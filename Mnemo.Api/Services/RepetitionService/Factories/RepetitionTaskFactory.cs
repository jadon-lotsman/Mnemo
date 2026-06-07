using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Mnemo.Data.Entities;
using Mnemo.Services.RepetitionService.Providers.DestructorProviders;

namespace Mnemo.Services.RepetitionService.Factories
{
    public class RepetitionTaskFactory
    {
        private readonly IDistractorProvider _provider;

        public RepetitionTaskFactory(IDistractorProvider provider)
        {
            _provider = provider;
        }

        public RepetitionTask Create(bool isForward, Type taskType, VocabularyEntry baseEntry, params int[] excludeIds)
        {
            if (taskType == typeof(OptionRepetitionTask))
                return CreateOptionsTask(isForward, baseEntry, excludeIds);
            if (taskType == typeof(OrderPartsRepetitionTask))
                return CreateExampleOrderPartsTask(baseEntry);
            if (taskType == typeof(YesOrNoRepetitionTask))
                return CreateYesOrNoTask(isForward, baseEntry, excludeIds);

            return CreateTextTask(isForward, baseEntry);
        }

        public TextRepetitionTask CreateTextTask(bool isForward, VocabularyEntry baseEntry)
        {
            string prompt = isForward ? baseEntry.Foreign : baseEntry.Translations[0];
            var correct = isForward ? baseEntry.Translations : [baseEntry.Foreign];

            return new TextRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, correct);
        }

        public OptionRepetitionTask CreateOptionsTask(bool isForward, VocabularyEntry baseEntry, params int[] excludeIds)
        {
            string prompt = isForward ? baseEntry.Foreign : baseEntry.Translations[0];
            var correct = isForward ? baseEntry.Translations[0] : baseEntry.Foreign;

            var options = _provider.GetDistructorsAsync(isForward, baseEntry.UserId, 3, excludeIds).Result;

            return new OptionRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, options, correct);
        }

        public OrderPartsRepetitionTask CreateExampleOrderPartsTask(VocabularyEntry baseEntry)
        {
            int index = Random.Shared.Next(baseEntry.Examples.Count);
            var sentence = baseEntry.Examples[index];

            return new OrderPartsRepetitionTask(baseEntry.UserId, baseEntry.Id, sentence);
        }

        public OrderPartsRepetitionTask CreateForeignOrderPartsTask(VocabularyEntry baseEntry)
        {
            var foreign = baseEntry.Foreign;

            return new OrderPartsRepetitionTask(baseEntry.UserId, baseEntry.Id, foreign);
        }

        public YesOrNoRepetitionTask CreateYesOrNoTask(bool isForward, VocabularyEntry baseEntry, params int[] excludeIds)
        {
            string prompt = baseEntry.Foreign;
            string option;

            bool isCorrect = Random.Shared.Next(2) == 0;
            if (isCorrect)
            {
                int index = Random.Shared.Next(baseEntry.Translations.Count);
                option = baseEntry.Translations[index];
            }
            else
            {
                option = _provider.GetDistructorsAsync(isForward, baseEntry.UserId, 1, excludeIds).Result.First();
            }

            return new YesOrNoRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, option, isCorrect);
        }
    }
}
