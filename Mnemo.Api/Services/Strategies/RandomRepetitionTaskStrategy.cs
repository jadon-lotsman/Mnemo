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

            var entries = _vocabularyQueries.GetRandomByUserId(userId, 10);
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
