using Microsoft.EntityFrameworkCore;
using Mnemo.Entities;
using Mnemo.Infrastructure.Queries;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class RandomDistractorProvider : IDistractorProvider
    {
        private GlossaryEntryQueries _vocabularyQueries;

        public RandomDistractorProvider(GlossaryEntryQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }

        public async Task<List<string>> GetDistractorsAsync(bool isForward, GlossaryEntry baseEntry, int take, params int[] excludeIds)
        {
            var entries = await _vocabularyQueries
                    .GetByOwnerIdQuery(baseEntry.OwnerId)
                    .GetRandomEntries(take, excludeIds)
                    .ToListAsync();

            var result = new List<string>();
            foreach (var entry in entries)
            {
                string distractor = isForward ? entry.Translations[0] : entry.Foreign;
                if (!result.Contains(distractor))
                    result.Add(distractor);
            }

            return result;
        }
    }
}
