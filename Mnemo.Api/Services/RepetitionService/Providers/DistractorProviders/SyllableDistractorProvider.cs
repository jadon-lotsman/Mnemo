using Mnemo.Data.Entities;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class SyllableDistractorProvider : IDistractorProvider
    {
        private readonly RandomDistractorProvider _randomDistractorProvider;
        private readonly HashSet<string> _helpingEnDistractors = new() { "tion", "de", "nth", "ture", "rse", "ment", "ness", "ful", "able", "un", "er" };
        private readonly HashSet<string> _helpingRuDistractors = new() { "пере", "на", "де", "еть", "ый", "ять" };

        public SyllableDistractorProvider(RandomDistractorProvider randomDistractorProvider)
        {
            _randomDistractorProvider = randomDistractorProvider;
        }

        public async Task<List<string>> GetDistractorsAsync(bool isForward, VocabularyEntry baseEntry, int take, params int[] excludeIds)
        {
            var result = new List<string>();

            var randomDistractors = await _randomDistractorProvider
                .GetDistractorsAsync(isForward, baseEntry, take, excludeIds);

            foreach (var distractor in randomDistractors)
            {
                var syllables = distractor.SplitIntoChunks(3);
                int index = Random.Shared.Next(1, syllables.Length);

                if (syllables.Length - 1 <= 1)
                    continue;

                result.Add(syllables[index]);
            }

            if (result.Count < take)
            {
                var randomHelping = new List<string>();

                if (isForward)
                {
                    randomHelping = _helpingRuDistractors
                        .OrderBy(x => Random.Shared.Next())
                        .Take(take - result.Count)
                        .ToList();
                }
                else
                {
                    randomHelping = _helpingEnDistractors
                        .OrderBy(x => Random.Shared.Next())
                        .Take(take - result.Count)
                        .ToList();
                }

                result.AddRange(randomHelping);
            }

            return result;
        }
    }
}
