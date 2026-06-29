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
    public class PlannedRepetitionTaskStrategy : RepetitionTaskStrategy
    {
        private readonly VocabularyQueries _vocabularyQueries;

        public PlannedRepetitionTaskStrategy(
            ILogger<PlannedRepetitionTaskStrategy> logger,
            RepetitionTaskFactory factory,
            ITaskTypeProvider typeProvider,
            VocabularyQueries vocabularyQueries) : base(logger, factory, typeProvider)
        {
            _vocabularyQueries = vocabularyQueries;
        }


        protected override async Task<IQueryable<VocabularyEntry>> GetEntriesQuery(int userId, int take)
        {
            var query = _vocabularyQueries
                .GetByUserIdQuery(userId)
                .Include(e => e.RepetitionState)
                .DueEntries()
                .OrderBy(e => e.Id)
                .Take(10);

            return query;
        }
    }
}
