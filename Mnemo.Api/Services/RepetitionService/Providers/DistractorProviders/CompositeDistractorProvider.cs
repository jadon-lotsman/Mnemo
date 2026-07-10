using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class CompositeDistractorProvider : IDistractorProvider
    {

        private readonly AntonymDistractorProvider _antonymDistractorProvider;
        private readonly ByPartOfSpeechDistractorProvider _byPartOfSpeechDistractorProvider;
        private readonly RandomDistractorProvider _randomDistractorProvider;

        public CompositeDistractorProvider(AntonymDistractorProvider antonymDistractorProvider, ByPartOfSpeechDistractorProvider byPartOfSpeechDistractorProvider, RandomDistractorProvider randomDistractorProvider)
        {
            _antonymDistractorProvider = antonymDistractorProvider;
            _byPartOfSpeechDistractorProvider = byPartOfSpeechDistractorProvider;
            _randomDistractorProvider = randomDistractorProvider;
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, VocabularyEntry baseEntry, int take, params int[] excludeIds)
        {
            var antonymDistractors = await _antonymDistractorProvider
                .GetDistractorsAsync(isForward, baseEntry, 1, excludeIds);

            var result = antonymDistractors;

            if (result.Count < take)
            {
                var partOfSpeechDistractors = await _byPartOfSpeechDistractorProvider
                    .GetDistractorsAsync(isForward, baseEntry, take - result.Count, excludeIds);

                result.AddRange(partOfSpeechDistractors);
            }

            if (result.Count < take)
            {
                var randomDistractors = await _randomDistractorProvider
                    .GetDistractorsAsync(isForward, baseEntry, take - result.Count, excludeIds);

                result.AddRange(randomDistractors);
            }

            if (result.Count < take)
                return [];

            return result;
        }
    }
}
