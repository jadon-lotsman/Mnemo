using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class CompositeDistractorProvider : IDistractorProvider
    {

        private readonly AntonymDistractorProvider _antonymDistractorProvider;
        private readonly PrefixDistractorProvider _prefixDistractorProvider;
        private readonly ByPartOfSpeechDistractorProvider _byPartOfSpeechDistractorProvider;
        private readonly RandomDistractorProvider _randomDistractorProvider;

        public CompositeDistractorProvider(AntonymDistractorProvider antonymDistractorProvider, PrefixDistractorProvider prefixDistractorProvider, ByPartOfSpeechDistractorProvider byPartOfSpeechDistractorProvider, RandomDistractorProvider randomDistractorProvider)
        {
            _antonymDistractorProvider = antonymDistractorProvider;
            _prefixDistractorProvider = prefixDistractorProvider;
            _byPartOfSpeechDistractorProvider = byPartOfSpeechDistractorProvider;
            _randomDistractorProvider = randomDistractorProvider;
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, VocabularyEntry baseEntry, int take, params int[] excludeIds)
        {
            var result = new List<string>();

            if (baseEntry.Antonyms.Any() && Random.Shared.NextDouble() <= 0.4d)
            {
                var antonymDistractors = await _antonymDistractorProvider
                    .GetDistractorsAsync(isForward, baseEntry, 1, excludeIds);

                result.AddRange(antonymDistractors);
            }

            if (result.Count < take && Random.Shared.NextDouble() <= 0.7d)
            {
                var prefixDistractors = await _prefixDistractorProvider
                    .GetDistractorsAsync(isForward, baseEntry, 1, excludeIds);

                result.AddRange(prefixDistractors);
            }

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

            return result;
        }
    }
}
