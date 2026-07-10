using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Queries;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class CompositeDistractorProvider : IDistractorProvider
    {
        private AntonymDistractorProvider _antonymDistractorProvider;
        private RandomByPartOfSpeechProvider _partOfSpeechDistractorProvider;
        private RandomDistractorProvider _randomDistractorProvider;

        public CompositeDistractorProvider(AntonymDistractorProvider antonymDistractorProvider, RandomByPartOfSpeechProvider partOfSpeechDistractorProvider, RandomDistractorProvider randomDistractorProvider)
        {
            _antonymDistractorProvider = antonymDistractorProvider;
            _partOfSpeechDistractorProvider = partOfSpeechDistractorProvider;
            _randomDistractorProvider = randomDistractorProvider;
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, int userId, int entryId, int take, params int[] excludeIds)
        {
            var priorityDistractors = await _antonymDistractorProvider
                .GetDistractorsAsync(isForward, userId, entryId, 1, excludeIds);


            var result = priorityDistractors;

            if (result.Count() < take)
            {
                var randomDistractors = await _partOfSpeechDistractorProvider
                    .GetDistractorsAsync(isForward, userId, entryId, take - priorityDistractors.Count, excludeIds);

                result.AddRange(randomDistractors);
            }

            if (result.Count() < take)
            {
                var randomDistractors = await _randomDistractorProvider
                    .GetDistractorsAsync(isForward, userId, entryId, take - priorityDistractors.Count, excludeIds);

                result.AddRange(randomDistractors);
            }

            if (result.Count < take)
                return [];

            return result;
        }
    }
}
