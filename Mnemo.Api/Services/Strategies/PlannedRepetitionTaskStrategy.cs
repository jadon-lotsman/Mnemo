using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Services.Factories;
using Mnemo.Services.Queries;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.Strategies
{
    public class PlannedRepetitionTaskStrategy : IRepetitionTaskStrategy
    {
        private VocabularyQueries _vocabularyQueries;

        public PlannedRepetitionTaskStrategy(VocabularyQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }

        public async Task<List<RepetitionTask>> GetTasksAsync(int userId)
        {
            var taskFactory = new RepetitionTaskFactory();

            var targetEntries = await _vocabularyQueries
                .GetRandomByUserIdQuery(userId)
                .Include(e => e.RepetitionState)
                .DueEntries()
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
