using System.Diagnostics;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Mnemo.Data;
using Mnemo.Services.EnrichmentService.ExternalDictionaries;
using Mnemo.Shared;

namespace Mnemo.Services.EnrichmentService
{
    public class EnrichmentBackgroundService : BackgroundService
    {
        private readonly ILogger<EnrichmentBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public EnrichmentBackgroundService(
            ILogger<EnrichmentBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }


        private static readonly int _batchSize = 5;
        private static readonly TimeSpan _batchDelay = TimeSpan.FromMinutes(2.5);


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Enrichment service started: delay {Delay} sec", _batchDelay.TotalSeconds);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessBatchAsync(stoppingToken);
                    await Task.Delay(_batchDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopping gracefully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Fatal error in enrichment service: {Message}", ex);
                throw;
            }
        }

        private async Task ProcessBatchAsync(CancellationToken stoppingToken)
        {
            List<long>? capturedIds = null;
            string? idsForCapture = null;

            const int processing = (int) EnrichmentStatus.Processing;
            const int pending = (int) EnrichmentStatus.Pending;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var externalDictionary = scope.ServiceProvider.GetRequiredService<IExternalDictionary>();

            // Reset
            //await context.Database
            //    .ExecuteSqlRawAsync(
            //    $"UPDATE \"Entries\" SET \"EnrichmentStatus\" = {pending}",
            //    stoppingToken);

            try
            {
                List<int> pendingIds = await context.Entries
                    .Where(e => e.EnrichmentStatus == EnrichmentStatus.Pending)
                    .OrderBy(e => e.Id)
                    .Select(e => e.Id)
                    .Take(_batchSize)
                    .ToListAsync(stoppingToken);

                if (!pendingIds.Any())
                    return;


                idsForCapture = string.Join(",", pendingIds);
                capturedIds = await context.Database
                    .SqlQueryRaw<long>(
                        $"UPDATE \"Entries\" SET \"EnrichmentStatus\" = {processing} " +
                        $"WHERE \"Id\" IN ({idsForCapture}) AND \"EnrichmentStatus\" = {pending} " +
                        $"RETURNING \"Id\"")
                    .ToListAsync(stoppingToken);

                if (capturedIds.Count < pendingIds.Count)
                    _logger.LogWarning("Captured only {CapturedCount} out of {PendingCount} entries. Others were already taken or status changed.", capturedIds.Count, pendingIds.Count);


                if (!capturedIds.Any())
                    return;


                var entries = await context.Entries
                    .Where(e => capturedIds.Contains(e.Id))
                    .ToListAsync(stoppingToken);


                foreach (var entry in entries)
                {
                    var stopwatch = Stopwatch.StartNew();

                    var result = await externalDictionary.GetEnrichAsync(entry.Foreign, entry.PartOfSpeech);
                    stopwatch.Stop();

                    _logger.LogInformation("External call for entry (EntryId:{EntryId}) took {ElapsedMs} ms", entry.Id, stopwatch.ElapsedMilliseconds);

                    entry.LastEnrichmentAt = DateTime.UtcNow;

                    if (!result.IsSuccess)
                    {
                        entry.EnrichmentStatus = EnrichmentStatus.Failed;
                        continue;
                    }

                    var enrichResponse = result.Value;

                    if (enrichResponse != null)
                    {
                        if (entry.Transcription == null && enrichResponse.Transcription != null)
                            entry.Transcription = enrichResponse.Transcription;

                        if ((entry.TranscriptionAudioUrl == null || entry.Transcription == enrichResponse.Transcription) && enrichResponse.TranscriptionAudioUrl != null)
                            entry.TranscriptionAudioUrl = enrichResponse.TranscriptionAudioUrl;

                        if (enrichResponse.Synonyms != null)
                            entry.Synonyms = enrichResponse.Synonyms.ToList();

                        if (enrichResponse.Antonyms != null)
                            entry.Antonyms = enrichResponse.Antonyms.ToList();
                    }

                    entry.EnrichmentStatus = EnrichmentStatus.Completed;
                }


                var completedCount = entries.Count(e => e.EnrichmentStatus == EnrichmentStatus.Completed);
                var failedCount = entries.Count(e => e.EnrichmentStatus == EnrichmentStatus.Failed);
                _logger.LogInformation("Batch is ending (Completed:{Completed}, Failed:{Failed}). Saving changes...", completedCount, failedCount);

                await context.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Successfully processed batch (Count:{Count})", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError("Fatal error in batch processing: {Message}", ex);

                if (capturedIds != null && capturedIds.Any())
                {
                    int updated = await context.Database
                        .ExecuteSqlRawAsync(
                        $"UPDATE \"Entries\" SET \"EnrichmentStatus\" = {pending} " +
                        $"WHERE \"Id\" IN ({idsForCapture}) AND \"EnrichmentStatus\" = {processing}",
                        stoppingToken);

                    _logger.LogInformation("Reset stuck entries (Count:{Count}) from Processing to Pending", updated);
                }
            }
        }
    }
}
