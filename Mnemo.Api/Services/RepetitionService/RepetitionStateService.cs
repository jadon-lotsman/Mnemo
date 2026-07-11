using Mnemo.Contracts.Repetition;
using Mnemo.Data;
using Mnemo.Data.Queries;
using Mnemo.Shared;

namespace Mnemo.Services.RepetitionService
{
    public class RepetitionStateService
    {
        private readonly ILogger<RepetitionStateService> _logger;
        private readonly AppDbContext _context;
        private readonly StateQueries _stateQueries;


        public RepetitionStateService(ILogger<RepetitionStateService> logger, AppDbContext context, StateQueries stateQueries)
        {
            _logger = logger;
            _context = context;
            _stateQueries = stateQueries;
        }


        public async Task<List<RepetitionDateResponse>> GetScheduleAsync(int userId)
        {
            var states = await _stateQueries.GetAllByUserIdAsync(userId);

            var grouped = states
                .GroupBy(s => s.NextRepetitionAt)
                .Select(d => new RepetitionDateResponse
                {
                    Date = d.Key,
                    VocabularyForeigns = d.Select(s => s.VocabularyEntry.Foreign).ToArray()
                })
                .OrderBy(d => d.Date)
                .ToList();

            return grouped;
        }


        public async Task<RequestResult<bool>> RecordQualityRepetitionStateAsync(int userId, Dictionary<int, double> entryIdToQuality)
        {
            _logger.LogInformation("Attempting to record quality for {Count} entries from user (UserId:{UserId})", entryIdToQuality.Count, userId);

            if (entryIdToQuality == null || !entryIdToQuality.Any())
            {
                _logger.LogWarning("Entry-to-quality dictionary is empty from user (UserId:{UserId})", userId);
                return RequestResult<bool>.Failure(ErrorCode.InvalidData, "Entry-to-quality is empty");
            }


            var stateDict = await _stateQueries.GetDictByEntryIdsAsync(userId, entryIdToQuality.Keys);

            if (stateDict.Count == 0)
            {
                _logger.LogWarning("States pull is empty from user (UserId:{UserId})", userId);
                return RequestResult<bool>.Success(false);
            }


            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var errors = new List<string>();

            if (entryIdToQuality.Count != stateDict.Count)
                errors.Add($"Not all states was found (Found:{stateDict.Count}, Total:{entryIdToQuality.Count})");

            foreach (var state in stateDict.Values)
            {
                if (!entryIdToQuality.TryGetValue(state.VocabularyEntryId, out double quality))
                    continue;

                if (!state.TryRecordQuality(quality, today, out string? errorMessage))
                    errors.Add($"Entry {state.VocabularyEntryId} failed: {errorMessage}");
            }

            _logger.LogInformation("Quality recording entries is ending from user (UserId:{UserId}). Saving changes...", userId);
            await _context.SaveChangesAsync();

            if (errors.Any())
            {
                var joined = string.Join("; ", errors);
                _logger.LogWarning("Success, but quality recording failed for {Count} entries: {Errors}", errors.Count, joined);
                return RequestResult<bool>.Failure(ErrorCode.ActionNotAllowed, joined);
            }


            _logger.LogInformation("Successfully recorded qualities for {Count} entries from user (UserId:{UserId})", stateDict.Count, userId);

            return RequestResult<bool>.Success(true);
        }
    }
}
