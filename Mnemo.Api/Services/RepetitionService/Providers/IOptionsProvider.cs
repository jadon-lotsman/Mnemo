using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Providers
{
    public interface IOptionsProvider
    {
        Task<List<VocabularyEntry>> GetOptionsAsync(int userId, int take = 4, params int[] excludeIds);
    }
}
