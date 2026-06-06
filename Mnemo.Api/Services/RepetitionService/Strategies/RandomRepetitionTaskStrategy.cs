using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService.Factories;
using Mnemo.Services.RepetitionService.Providers;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Strategies
{
    public class RandomRepetitionTaskStrategy : IRepetitionTaskStrategy
    {
        private VocabularyQueries _vocabularyQueries;
        private RepetitionTaskFactory _factory;

        public RandomRepetitionTaskStrategy(VocabularyQueries vocabularyQueries, RepetitionTaskFactory factory)
        {
            _vocabularyQueries = vocabularyQueries;
            _factory = factory;
        }

        public async Task<List<RepetitionTask>> GetTasksAsync(int userId)
        {
            var targetEntries = await _vocabularyQueries
                .GetRandomByUserIdQuery(userId, 5)
                .Include(e => e.RepetitionState)
                .NotDueEntries()
                .ToListAsync();

            var tasks = new List<RepetitionTask>();
            var excludeIds = targetEntries.Select(e => e.Id).ToArray();

            foreach (var entry in targetEntries)
            {
                var task = await GetOneTaskAsync(userId, entry, excludeIds);
                tasks.Add(task);
            }

            return tasks;
        }

        private async Task<RepetitionTask> GetOneTaskAsync(int userId, VocabularyEntry entry, int[] excludeIds)
        {
            bool isForward = Random.Shared.Next(2) == 0;
            var distructors = await new RandomDistructorProvider(_vocabularyQueries).GetDistructorsAsync(userId, 3, excludeIds);

            int rnd = Random.Shared.Next(100);

            if (rnd < 30 && distructors.Count >= 3)
                return _factory.CreateOptionsTask(isForward, entry, distructors);
            if (rnd < 50 && entry.Foreign.Length > 4)
                return _factory.CreateOrderPartsTask(entry);
            if (rnd < 75 || !distructors.Any())
                return _factory.CreateTextTask(isForward, entry);
            return _factory.CreateYesOrNoTask(entry, distructors[0]);
        }
    }
}
