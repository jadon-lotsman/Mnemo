using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService.Factories;

namespace Mnemo.Services.RepetitionService.Providers
{
    public class RandomDistructorProvider : IDistructorProvider
    {
        private VocabularyQueries _vocabularyQueries;

        public RandomDistructorProvider(VocabularyQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }

        public async Task<List<VocabularyEntry>> GetDistructorsAsync(int userId, int take, params int[] excludeIds)
            => await _vocabularyQueries
                    .GetRandomByUserIdQuery(userId, take, excludeIds)
                    .ToListAsync();
    }
}
