using Mnemo.Contracts.Dtos.Repetition.Responses;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;
using Mnemo.Shared;

namespace Mnemo.Services
{
    public class RepetitionStateService
    {
        private AppDbContext _context;
        private StateQueries _stateQueries;
        private VocabularyQueries _vocabularyQueries;


        public RepetitionStateService(AppDbContext context, StateQueries stateQueries, VocabularyQueries vocabularyQueries)
        {
            _context = context;
            _stateQueries = stateQueries;
            _vocabularyQueries = vocabularyQueries;
        }


        public async Task<List<RepetitionDayResponse>> GetScheduleAsync(int userId)
        {
            var states = await _stateQueries.GetAllByUserIdAsync(userId);

            var grouped = states
                .GroupBy(s => s.NextRepetitionAt)
                .Select(d => new RepetitionDayResponse
                {
                    Date = d.Key,
                    VocabularyForeigns = d.Select(s => s.VocabularyEntry.Foreign).ToArray()
                })
                .OrderBy(d => d.Date)
                .ToList();

            return grouped;
        }


        public async Task<RequestResult<bool>> CreateNonExistentRepetitionStatesAsync(int userId)
        {
            var entries = await _vocabularyQueries.GetEntriesWithoutRepetitionStateAsync(userId);

            if (!entries.Any())
                return RequestResult<bool>.Success(false);


            var states = entries.Select(e => new RepetitionState(userId, e.Id)).ToList();
            await _context.RepetitionStates.AddRangeAsync(states);
            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }


        public async Task<RequestResult<bool>> RecordQualityRepetitionStateAsync(int userId, Dictionary<int, double> entryIdToQuality, bool isSelfAssess = false)
        {
            if (entryIdToQuality == null || !entryIdToQuality.Any())
                return RequestResult<bool>.Failure(ErrorCode.InvalidData, "Entry-quality is empty");


            var stateDict = await _stateQueries.GetDictByEntryIdsAsync(userId, entryIdToQuality.Keys);

            if (stateDict.Count == 0)
                return RequestResult<bool>.Success(false);


            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var state in stateDict.Values)
            {
                if (!entryIdToQuality.TryGetValue(state.VocabularyEntryId, out double quality))
                    continue;

                if(!state.TryRecordQuality(quality, isSelfAssess, today, out string? errorMessage))
                    return RequestResult<bool>.Failure(isSelfAssess ? ErrorCode.ActionNotAllowed : ErrorCode.InvalidData, errorMessage);
            }

            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }
    }
}
