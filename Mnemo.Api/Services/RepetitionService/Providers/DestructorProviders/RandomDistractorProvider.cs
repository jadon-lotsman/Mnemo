using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService.Factories;

namespace Mnemo.Services.RepetitionService.Providers.DestructorProviders
{
    public class RandomDistractorProvider : IDistractorProvider
    {
        private VocabularyQueries _vocabularyQueries;

        public RandomDistractorProvider(VocabularyQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }

        public async Task<List<string>> GetDistructorsAsync(bool isForward, int userId,  int take, params int[] excludeIds)
        { 
            var entries = await _vocabularyQueries
                    .GetRandomByUserIdQuery(userId, take, excludeIds)
                    .ToListAsync();

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
