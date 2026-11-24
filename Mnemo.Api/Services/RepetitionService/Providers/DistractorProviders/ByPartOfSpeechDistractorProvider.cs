using Microsoft.EntityFrameworkCore;
using Mnemo.Entities;
using Mnemo.Infrastructure.Queries;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class ByPartOfSpeechDistractorProvider : IDistractorProvider
    {
        private GlossaryEntryQueries _entryQueries;

        public ByPartOfSpeechDistractorProvider(GlossaryEntryQueries entryQueries)
        {
            _entryQueries = entryQueries;
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, GlossaryEntry baseEntry, int take, params Guid[] excludeIds)
        {
            var entries = await _entryQueries
                    .GetByOwnerIdQuery(baseEntry.OwnerId)
                    .Where(e => baseEntry.PartOfSpeech != null && e.PartOfSpeech == baseEntry.PartOfSpeech)
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
