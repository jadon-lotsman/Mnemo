using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Providers.TaskTypeProviders
{
    public class FixedOrderTaskTypeProvider : ITaskTypeProvider
    {
        private readonly Type[] _order = { typeof(TextRepetitionTask), typeof(OptionRepetitionTask), typeof(SentenceReorderRepetitionTask), typeof(SyllableReorderRepetitionTask), typeof(YesOrNoRepetitionTask) };
        private int _index = 0;


        public (Type taskType, bool isForward) GetType(double easinessFactor)
        {
            return (_order[_index++ % _order.Length], true);
        }
    }
}
