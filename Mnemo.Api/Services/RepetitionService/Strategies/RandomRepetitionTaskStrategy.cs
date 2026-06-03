using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService.Factories;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Strategies
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
                .GetRandomByUserIdQuery(userId, 5)
                .Include(e => e.RepetitionState)
                .NotDueEntries()
                .ToListAsync();

            var tasks = new List<RepetitionTask>();

            foreach (var entry in targetEntries)
            {
                int[] arrayIds = targetEntries.Select(e => e.Id).ToArray();

                var entriesForOptions = await _vocabularyQueries
                    .GetRandomByUserIdQuery(userId, 3, arrayIds)
                    .ToListAsync();

                tasks.Add(taskFactory.Create(entry, entriesForOptions));
            }

            return tasks;
        }
    }
}
