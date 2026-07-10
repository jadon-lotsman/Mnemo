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
    public class FastRepetitionTaskStrategy : RepetitionTaskStrategy
    {
        private readonly VocabularyQueries _vocabularyQueries;

        public FastRepetitionTaskStrategy(
            ILogger<FastRepetitionTaskStrategy> logger,
            RepetitionTaskFactory factory,
            ITaskTypeProvider typeProvider,
            VocabularyQueries vocabularyQueries) : base(logger, factory, typeProvider)
        {
            _vocabularyQueries = vocabularyQueries;
        }


        protected override async Task<IQueryable<VocabularyEntry>> GetEntriesQuery(int userId, int take)
        {
            var priorityEntriesQuery = _vocabularyQueries
                .GetByUserIdQuery(userId)
                .Include(e => e.RepetitionState)
                .NotDueEntries()
                .NotRepeatedTodayEntries()
                .OrderBy(e => e.Id)
                .Take(take);


            var mixQuery = priorityEntriesQuery;

            if (priorityEntriesQuery.Count() < take)
            {
                var existingIds = priorityEntriesQuery.Select(e => e.Id).ToArray();

                var randomEntries = _vocabularyQueries
                    .GetByUserIdQuery(userId)
                    .GetRandomEntries(take - existingIds.Length, existingIds)
                    .Include(e => e.RepetitionState)
                    .NotDueEntries();

                mixQuery = mixQuery.Concat(randomEntries);
            }


            return mixQuery;
        }
    }
}
