using Microsoft.EntityFrameworkCore;
using Mnemo.Common;
using Mnemo.Contracts.Dtos.Repetition;
using Mnemo.Data.Entities;
using Mnemo.Services.Factories;
using Mnemo.Services.Queries;

namespace Mnemo.Services.Strategies
{
    public class RandomRepetitionTaskStrategy : IRepetitionTaskStrategy
    {
        private VocabularyQueries _vocabularyQueries;

        public RandomRepetitionTaskStrategy(VocabularyQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }

        public async Task<List<RepetitionTask>> GetTasksAsync(int userId)
        {
            var taskFactory = new RepetitionTaskFactory();

            var targetEntries = await _vocabularyQueries
                .GetRandomByUserIdQuery(userId, 10)
                .Include(e => e.RepetitionState)
                .NotDueEntries()
                .ToListAsync();

            var tasks = new List<RepetitionTask>();

            foreach (var entry in targetEntries)
            {
                var entriesForOptions = await _vocabularyQueries
                    .GetRandomByUserIdQuery(userId, 3, entry.Id)
                    .ToListAsync();

                tasks.Add(taskFactory.Create(entry, entriesForOptions));
            }

            return tasks;
        }
    }
}
