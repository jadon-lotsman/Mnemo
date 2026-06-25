using Mnemo.Contracts.Repetition;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Shared;

namespace Mnemo.Services.RepetitionService
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
            if (entryIdToQuality == null || !entryIdToQuality.Any())
                return RequestResult<bool>.Failure(ErrorCode.InvalidData, "Entry-quality is empty");


            var stateDict = await _stateQueries.GetDictByEntryIdsAsync(userId, entryIdToQuality.Keys);

            if (stateDict.Count == 0)
                return RequestResult<bool>.Success(false);


            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            RequestResult<bool>? errorResult = null;

            if (entryIdToQuality.Count != stateDict.Count)
                errorResult = RequestResult<bool>.Failure(ErrorCode.StateNotFound);

            foreach (var state in stateDict.Values)
            {
                if (!entryIdToQuality.TryGetValue(state.VocabularyEntryId, out double quality))
                    continue;

                if (!state.TryRecordQuality(quality, today, out string? errorMessage) && errorResult == null)
                    errorResult = RequestResult<bool>.Failure( ErrorCode.InvalidData, errorMessage);
            }

            await _context.SaveChangesAsync();

            return errorResult ?? RequestResult<bool>.Success(true);
        }
    }
}
