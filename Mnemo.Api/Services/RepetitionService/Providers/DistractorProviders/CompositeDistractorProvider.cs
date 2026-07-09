using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Queries;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class CompositeDistractorProvider : IDistractorProvider
    {
        private VocabularyQueries _vocabularyQueries;
        private AntonymDistractorProvider _antonymDistractorProvider;
        private RandomDistractorProvider _randomDistractorProvider;

        public CompositeDistractorProvider(VocabularyQueries vocabularyQueries, AntonymDistractorProvider antonymDistractorProvider, RandomDistractorProvider randomDistractorProvider)
        {
            _vocabularyQueries = vocabularyQueries;
            _antonymDistractorProvider = antonymDistractorProvider;
            _randomDistractorProvider = randomDistractorProvider;
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, int userId, int entryId, int take, params int[] excludeIds)
        {
            var priorityDistractors = await _antonymDistractorProvider
                .GetDistractorsAsync(isForward, userId, entryId, 1, excludeIds);


            var result = priorityDistractors;

            if (priorityDistractors.Count() < take)
            {
                var randomDistractors = await _randomDistractorProvider
                    .GetDistractorsAsync(isForward, userId, entryId, take - priorityDistractors.Count, excludeIds);

                result.AddRange(randomDistractors);
            }

            return result;
        }
    }
}
