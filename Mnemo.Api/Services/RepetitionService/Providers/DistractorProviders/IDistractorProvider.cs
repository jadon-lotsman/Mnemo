using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Providers.DistractorProviders
{
    public interface IDistractorProvider
    {
        Task<List<string>> GetDistractorsAsync(bool isForward, GlossaryEntry baseEntry, int take, params int[] excludeIds);
    }
}
