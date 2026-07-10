using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class ByPartOfSpeechDistractorProvider : IDistractorProvider
    {
        private VocabularyQueries _vocabularyQueries;

        public ByPartOfSpeechDistractorProvider(VocabularyQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, VocabularyEntry baseEntry, int take, params int[] excludeIds)
        {
            var entries = await _vocabularyQueries
                    .GetByUserIdQuery(baseEntry.UserId)
                    .Where(e => baseEntry.PartOfSpeech != null && e.PartOfSpeech == baseEntry.PartOfSpeech)
                    .GetRandomEntries(take, excludeIds)
                    .ToListAsync();

            if (entries.Count < take)
                return [];

            var result = new List<string>();
            foreach (var entry in entries)
            {
                string distructor = isForward ? entry.Translations[0] : entry.Foreign;
                if (!result.Contains(distructor))
                    result.Add(distructor);
            }

            return result;
        }
    }
}
