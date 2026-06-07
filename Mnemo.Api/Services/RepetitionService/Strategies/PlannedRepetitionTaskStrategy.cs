using System.Threading.Tasks;
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
    public class PlannedRepetitionTaskStrategy : IRepetitionTaskStrategy
    {
        private readonly RepetitionTaskFactory _factory;
        private readonly ITaskTypeProvider _typeProvider;
        private readonly VocabularyQueries _vocabularyQueries;

        public PlannedRepetitionTaskStrategy(RepetitionTaskFactory factory, ITaskTypeProvider typeProvider, VocabularyQueries vocabularyQueries)
        {
            _factory = factory;
            _typeProvider = typeProvider;
            _vocabularyQueries = vocabularyQueries;
        }

        public async Task<List<RepetitionTask>> GetTasksAsync(int userId)
        {
            var targetEntries = await _vocabularyQueries
                .GetRandomByUserIdQuery(userId, 10)
                .Include(e => e.RepetitionState)
                .DueEntries()
                .ToListAsync();

            var tasks = new List<RepetitionTask>();
            var excludeIds = targetEntries.Select(e => e.Id).ToArray();

            foreach (var entry in targetEntries)
            {
                double easeFactor = entry.RepetitionState?.EasinessFactor ?? SM2Helper.InitEF;

                (Type taskType, bool isForward) = _typeProvider.GetType(easeFactor);
                var task = _factory.Create(isForward, taskType, entry, excludeIds);

                tasks.Add(task);
            }

            return tasks;
        }
    }
}
