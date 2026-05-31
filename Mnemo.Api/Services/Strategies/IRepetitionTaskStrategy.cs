using Mnemo.Data.Entities;

namespace Mnemo.Services.Strategies
{
    public interface IRepetitionTaskStrategy
    {
        Task<List<RepetitionTask>> GetTasksAsync(int userId);
    }
}
