using Mnemo.Data.Entities;
using Mnemo.Data.Queries;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public class AntonymDistractorProvider : IDistractorProvider
    {
        public AntonymDistractorProvider()
        {
        }


        public async Task<List<string>> GetDistractorsAsync(bool isForward, VocabularyEntry baseEntry, int take, params int[] excludeIds)
        {
            if (isForward)
                return [];

            if (baseEntry.Antonyms.Count < take)
                return [];

            var antonyms = baseEntry
                .Antonyms
                .OrderBy(x => Random.Shared.Next())
                .ToList();

            return antonyms
                .Take(take)
                .ToList();
        }
    }
}
