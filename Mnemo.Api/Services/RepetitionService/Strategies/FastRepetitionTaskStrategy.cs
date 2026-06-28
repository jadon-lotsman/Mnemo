using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService.Factories;
using Mnemo.Services.RepetitionService.Providers;
using Mnemo.Services.RepetitionService.Providers.TaskTypeProviders;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Strategies
{
    public class FastRepetitionTaskStrategy : IRepetitionTaskStrategy
    {
        private readonly RepetitionTaskFactory _factory;
        private readonly ITaskTypeProvider _typeProvider;
        private readonly VocabularyQueries _vocabularyQueries;

        public FastRepetitionTaskStrategy(RepetitionTaskFactory factory, ITaskTypeProvider typeProvider, VocabularyQueries vocabularyQueries)
        {
            _factory = factory;
            _typeProvider = typeProvider;
            _vocabularyQueries = vocabularyQueries;
        }

        public async Task<List<RepetitionTask>> GetTasksAsync(int userId)
        {
            int count = 10;

            var priorityEntriesQuery = _vocabularyQueries
                .GetByUserIdQuery(userId)
                .Include(e => e.RepetitionState)
                .NotDueEntries()
                .NotRepeatedTodayEntries()
                .OrderBy(e => e.Id)
                .Take(count);


            var mixQuery = priorityEntriesQuery;

            if (priorityEntriesQuery.Count() < count)
            {
                var existingIds = priorityEntriesQuery.Select(e => e.Id).ToArray();

                var randomEntries = _vocabularyQueries
                    .GetRandomByUserIdQuery(userId, count-existingIds.Length, existingIds)
                    .Include(e => e.RepetitionState)
                    .NotDueEntries();

                mixQuery = mixQuery.Concat(randomEntries);
            }


            var targetEntries = await mixQuery
                .OrderBy(e => EF.Functions.Random())
                .ToListAsync();


            var tasks = new List<RepetitionTask>();
            var index = 0;
            var excludeIds = targetEntries.Select(e => e.Id).ToArray();

            foreach (var entry in targetEntries)
            {
                double easeFactor = entry.RepetitionState?.EasinessFactor ?? SM2Helper.InitEF;

                (Type taskType, bool isForward) = _typeProvider.GetType(easeFactor);
                var task = await _factory.CreateByTypeAsync(isForward, taskType, entry, excludeIds);

                task.OrderIndex = index;
                index++;

                tasks.Add(task);
            }

            return tasks;
        }
    }
}
