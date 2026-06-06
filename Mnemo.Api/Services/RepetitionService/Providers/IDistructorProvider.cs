using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Providers
{
    public interface IDistructorProvider
    {
        Task<List<VocabularyEntry>> GetDistructorsAsync(int userId, int take, params int[] excludeIds);
    }
}
