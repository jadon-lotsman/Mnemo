using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mnemo.Entities;
using Mnemo.Infrastructure.Queries;
using Mnemo.Services.RepetitionService.Factories;
using Mnemo.Services.RepetitionService.Providers.TaskTypeProviders;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Strategies
{
    public class PlannedRepetitionTaskStrategy : RepetitionTaskStrategy
    {
        private readonly GlossaryEntryQueries _vocabularyQueries;

        public PlannedRepetitionTaskStrategy(
            IOptions<RepetitionOptions> options,
            IOptions<SM2Options> sm2,
            ILogger<PlannedRepetitionTaskStrategy> logger,
            RepetitionTaskFactory factory,
            ITaskTypeProvider typeProvider,
            GlossaryEntryQueries vocabularyQueries) : base(options, sm2, logger, factory, typeProvider)
        {
            _vocabularyQueries = vocabularyQueries;
        }


        protected override async Task<IQueryable<GlossaryEntry>> GetTargetEntriesQuery(int userId, int take)
        {
            var query = _vocabularyQueries
                .GetByOwnerIdQuery(userId)
                .Where(e => e.IsEnrolled)
                .DueEntries()
                .GetRandomEntries(take);

            return query;
        }
    }
}
