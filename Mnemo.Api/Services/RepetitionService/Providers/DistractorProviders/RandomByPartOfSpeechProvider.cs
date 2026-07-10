using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Queries;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class RandomByPartOfSpeechProvider : IDistractorProvider
    {
        private VocabularyQueries _vocabularyQueries;

        public RandomByPartOfSpeechProvider(VocabularyQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, int userId, int entryId, int take, params int[] excludeIds)
        {
            var taskEntry = await _vocabularyQueries
                    .GetByIdAsync(userId, entryId);

            if (taskEntry == null)
                return [];

            var entries = await _vocabularyQueries
                    .GetByUserIdQuery(userId)
                    .Where(e => taskEntry.PartOfSpeech != null && e.PartOfSpeech == taskEntry.PartOfSpeech)
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
