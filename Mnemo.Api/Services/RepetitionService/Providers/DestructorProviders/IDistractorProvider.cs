using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Providers.DestructorProviders
{
    public interface IDistractorProvider
    {
        Task<List<string>> GetDistructorsAsync(bool isForward, int userId, int take, params int[] excludeIds);
    }
}
