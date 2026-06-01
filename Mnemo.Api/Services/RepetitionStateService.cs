using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Contracts.Dtos.Repetition;
using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;

namespace Mnemo.Services
{
    public class RepetitionStateService
    {
        private AppDbContext _context;
        private StateQueries _stateQueries;


        public RepetitionStateService(AppDbContext context, StateQueries stateQueries)
        {
            _context = context;
            _stateQueries = stateQueries;
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
            var entries = await _stateQueries.GetEntriesWithoutRepetitionStateAsync(userId);

            if (!entries.Any())
                return RequestResult<bool>.Success(false);


            var states = entries.Select(e => new RepetitionState(userId, e)).ToList();
            await _context.RepetitionStates.AddRangeAsync(states);
            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }


        public async Task<RequestResult<bool>> SetQualityRepetitionStateAsync(int userId, Dictionary<int, double> entryIdToQuality, bool isSelfAssess = false)
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


                if (state.NextRepetitionAt > today)
                {
                    state.LastRepetitionAt = today;
                    continue;
                }

                if (quality < 0 || quality > 5)
                    return RequestResult<bool>.Failure(ErrorCode.InvalidData, $"Quality {quality} out of range 0...5");


                if (isSelfAssess)
                {
                    if (!state.CanSelfAssess)
                        return RequestResult<bool>.Failure(ErrorCode.ActionNotAllowed, "Self-assessment not allowed");

                    state.CanSelfAssess = false;
                }
                else
                {
                    state.RepetitionCounter = SM2Helper.IsPassingQuality(quality) ? state.RepetitionCounter + 1 : 0;
                    state.CanSelfAssess = SM2Helper.IsPassingQuality(quality);
                    state.LastRepetitionAt = today;
                }


                (int interval, double easinessFactor)
                    = SM2Helper.NextIntervalAndEf(state.EasinessFactor, state.RepetitionInterval, state.RepetitionCounter, quality);

                state.RepetitionInterval = interval;
                state.EasinessFactor = easinessFactor;
                state.NextRepetitionAt = state.LastRepetitionAt.AddDays(interval);
            }

            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }
    }
}
