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
                .GroupBy(s => s.LastRepetitionAt.AddDays(s.RepetitionInterval))
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


        public async Task<RequestResult<bool>> SetQualityRepetitionStateAsync(int userId, Dictionary<int, double> entryIdToQuality, bool isSelfAssess=false)
        {
            var stateDict = await _stateQueries.GetDictByEntryIdsAsync(userId, entryIdToQuality.Keys);

            if (!stateDict.Values.Any())
                return RequestResult<bool>.Success(false);


            foreach (var state in stateDict.Values)
            {
                if (entryIdToQuality.TryGetValue(state.VocabularyEntryId, out double quality))
                {
                    if (isSelfAssess)
                    {
                        if (!state.CanSelfAssess)
                            return RequestResult<bool>.Failure(ErrorCode.ActionNotAllowed);

                        state.CanSelfAssess = false;
                    }
                    else
                    {
                        state.RepetitionCounter = SM2Helper.IsPassingQuality(quality) ? state.RepetitionCounter + 1 : 0;
                        state.CanSelfAssess = SM2Helper.IsPassingQuality(quality);
                        state.LastRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
                    }


                    (int interval, double easinessFactor)
                        = SM2Helper.NextIntervalAndEf(state.EasinessFactor, state.RepetitionInterval, state.RepetitionCounter, quality);

                    state.RepetitionInterval = interval;
                    state.EasinessFactor = easinessFactor;
                }
            }

            await _context.SaveChangesAsync();

            Console.WriteLine($"Updated {stateDict.Count} states");

            return RequestResult<bool>.Success(true);
        }
    }
}
