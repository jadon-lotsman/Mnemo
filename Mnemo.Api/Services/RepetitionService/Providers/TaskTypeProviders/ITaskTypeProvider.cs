namespace Mnemo.Services.RepetitionService.Providers.TaskTypeProviders
{
    public interface ITaskTypeProvider
    {
        (Type taskType, bool isForward) GetType(double easeFactor);
    }
}
