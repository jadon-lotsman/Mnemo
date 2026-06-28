using Mnemo.Data.Entities;
using Mnemo.Shared;

namespace Mnemo.Services.RepetitionService.Providers.TaskTypeProviders
{
    public class WeightTaskTypeProvider : ITaskTypeProvider
    {
        public (Type taskType, bool isForward) GetType(double easeFactor)
        {
            var typeRand = Random.Shared.NextDouble();
            var forwardRand= Random.Shared.NextDouble();

            if (easeFactor < 1.8)
            {
                bool isForward = forwardRand <= 0.8;

                if (typeRand < 0.4) return (typeof(YesOrNoRepetitionTask), isForward);
                if (typeRand < 0.9) return (typeof(SyllableReorderRepetitionTask), isForward);
                return (typeof(OptionRepetitionTask), isForward);
            }
            else if (easeFactor < 2.3)
            {
                bool isForward = forwardRand <= 0.6;

                if (typeRand < 0.1) return (typeof(YesOrNoRepetitionTask), isForward);
                if (typeRand < 0.3) return (typeof(SyllableReorderRepetitionTask), isForward);
                if (typeRand < 0.6) return (typeof(OptionRepetitionTask), isForward);
                return (typeof(TextRepetitionTask), isForward);
            }
            else
            {
                bool isForward = forwardRand <= 0.5;

                if (typeRand < 0.2) return (typeof(OptionRepetitionTask), isForward);
                if (typeRand < 0.8) return (typeof(TextRepetitionTask), isForward);
                return (typeof(SentenceReorderRepetitionTask), isForward);
            }
        }
    }
}
