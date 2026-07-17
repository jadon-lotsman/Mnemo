using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mnemo.Contracts.Repetition;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService
{
    public class RepetitionStateService
    {
        private readonly IOptions<RepetitionOptions> _options;
        private readonly ILogger<RepetitionStateService> _logger;
        private readonly AppDbContext _context;
        private readonly StateQueries _stateQueries;


        public RepetitionStateService(
            IOptions<RepetitionOptions> options,
            ILogger<RepetitionStateService> logger,
            AppDbContext context,
            StateQueries stateQueries)
        {
            _options = options;
            _logger = logger;
            _context = context;
            _stateQueries = stateQueries;
        }


        public async Task<List<RepetitionDateResponse>> GetRepetitionScheduleAsync(int userId)
        {
            var states = _stateQueries
                .GetByUserIdQuery(userId)
                .Where(s => s.LastRepetitionAt != DateOnly.MinValue) // Exclude new entries with default RepetitionState
                .Include(s => s.VocabularyEntry);

            var dateToday = DateOnly.FromDateTime(DateTime.UtcNow);
            int daysUntilNext = DateTime.UtcNow.DaysUntilNext(DayOfWeek.Monday);

            var grouped = states
                .Where(s => s.NextRepetitionAt <= dateToday.AddDays(7 + daysUntilNext))
                .GroupBy(s => s.NextRepetitionAt)
                .Select(d => new RepetitionDateResponse
                {
                    Date = d.Key,
                    IsImportantDay = d.Average(s => s.EasinessFactor) < SM2Helper.ImportantDayEF || d.Count() > _options.Value.RepetitionTaskCount,
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

        public async Task BalanceRepetitionStateAsync(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var groups = _stateQueries
                .GetByUserIdQuery(userId)
                .GroupBy(s => s.NextRepetitionAt)
                .ToDictionary(g => g.Key, g => g.ToList());


            int maxPerDay = _options.Value.RepetitionTaskCount;

            bool changed;
            do
            {
                changed = false;

                var overfilled = groups
                    .Where(g => g.Value.Count() > maxPerDay)
                    .OrderByDescending(g => g.Value.Count())
                    .ThenBy(g => g.Key)
                    .ToList();


                foreach (var day in overfilled)
                {
                    var moveTo = day.Value
                        .OrderBy(s => s.EasinessFactor)
                        .Take(day.Value.Count - maxPerDay)
                        .ToList();

                    var succesfullyMoved = new List<RepetitionState>();

                    foreach (var state in moveTo)
                    {
                        DateOnly? nearest = FindNearestFreeDay(today, state.NextRepetitionAt, groups, maxPerDay);

                        if (nearest.HasValue)
                        {
                            state.NextRepetitionAt = nearest.Value;

                            if (!groups.ContainsKey(nearest.Value))
                                groups[nearest.Value] = new List<RepetitionState>();

                            groups[nearest.Value].Add(state);
                            succesfullyMoved.Add(state);
                            changed = true;
                        }
                    }

                    var toRemove = new HashSet<RepetitionState>(succesfullyMoved);
                    day.Value.RemoveAll(toRemove.Contains);
                }

            } while (changed);

            
            await _context.SaveChangesAsync();
        }

        private DateOnly? FindNearestFreeDay(DateOnly today, DateOnly original, Dictionary<DateOnly, List<RepetitionState>> groups, int maxCount, int maxOffset = 3)
        {
            for (int offset = 1; offset <= maxOffset; offset++)
            {
                var candidates = new[] { original.AddDays(offset), original.AddDays(-offset) };
                foreach (var date in candidates)
                {
                    if (date <= today) continue;
                    if (!groups.TryGetValue(date, out var list) || list.Count < maxCount)
                        return date;
                }
            }

            return null;
        }
    }
}
