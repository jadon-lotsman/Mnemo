using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Services.RepetitionService.Factories;

namespace Mnemo.Services.RepetitionService.Providers
{
    public class RandomOptionsProvider : IOptionsProvider
    {
        private VocabularyQueries _vocabularyQueries;

        public RandomOptionsProvider(VocabularyQueries vocabularyQueries)
        {
            _vocabularyQueries = vocabularyQueries;
        }

        public async Task<List<VocabularyEntry>> GetOptionsAsync(int userId, int take = 4, params int[] excludeIds)
            => await _vocabularyQueries
                    .GetRandomByUserIdQuery(userId, take, excludeIds)
                    .ToListAsync();
    }
}
