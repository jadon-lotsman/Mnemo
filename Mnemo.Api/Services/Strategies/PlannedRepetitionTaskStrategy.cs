using Mnemo.Data.Entities;
using Mnemo.Services.Factories;
using Mnemo.Services.Queries;

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

            var entries = _vocabularyQueries.GetDueByUserId(userId);
            var tasks = new List<RepetitionTask>();

            foreach (var entry in entries)
            {
                var entriesForOptions = _vocabularyQueries.GetRandomByUserId(userId, 3, entry.Id);

                tasks.Add(taskFactory.Create(entry, entriesForOptions));
            }

            return tasks;
        }
    }
}
