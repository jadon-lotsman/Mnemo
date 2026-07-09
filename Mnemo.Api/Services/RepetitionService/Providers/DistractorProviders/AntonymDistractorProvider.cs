using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Queries;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class AntonymDistractorProvider : IDistractorProvider
    {
        private VocabularyQueries _vocabularyQueries;

        public AntonymDistractorProvider(VocabularyQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, int userId, int entryId, int take, params int[] excludeIds)
        {
            if (isForward)
                return [];

            var entry = await _vocabularyQueries
                .GetByIdAsync(userId, entryId);

            var antonyms = entry
                .Antonyms
                .OrderBy(x => Random.Shared.Next())
                .ToList();

            if (antonyms.Count < take)
                return [];

            var result = antonyms
                .Take(take)
                .ToList();

            return result;
        }
    }
}
