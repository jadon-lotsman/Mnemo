using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Factories
{
    public class RepetitionTaskFactory
    {
        public RepetitionTask Create(VocabularyEntry baseEntry, List<VocabularyEntry> entriesForOptions)
        {
            bool isForward = Random.Shared.Next(2) == 0;
            int rnd = Random.Shared.Next(100);

            if (rnd < 30 && entriesForOptions.Count >= 3)
                return CreateOptionsTask(isForward, baseEntry, entriesForOptions);
            if (rnd < 50 && baseEntry.Examples.Any())
                return CreateOrderPartsTask(baseEntry);
            if (rnd < 75)
                return CreateTextTask(isForward, baseEntry);
            return CreateYesOrNoTask(isForward, baseEntry, entriesForOptions[0]);
        }

        public TextRepetitionTask CreateTextTask(bool isForward, VocabularyEntry baseEntry)
        {
            string prompt = isForward ? baseEntry.Foreign : baseEntry.Translations[0];
            var correct   = isForward ? baseEntry.Translations : [baseEntry.Foreign];

            return new TextRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, correct);
        }

        public OptionRepetitionTask CreateOptionsTask(bool isForward, VocabularyEntry baseEntry, List<VocabularyEntry> entriesForOptions)
        {
            string prompt = isForward ? baseEntry.Foreign : baseEntry.Translations[0];
            var correct = isForward ? baseEntry.Translations[0] : baseEntry.Foreign;

            var options = new List<string>();
            foreach (var entry in entriesForOptions)
            {
                string option = isForward ? entry.Translations[0] : entry.Foreign;
                if (!options.Contains(option) && correct != option)
                    options.Add(option);
            }

            return new OptionRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, options, correct);
        }

        public OrderPartsRepetitionTask CreateOrderPartsTask(VocabularyEntry baseEntry)
        {
            int index = Random.Shared.Next(baseEntry.Examples.Count);
            var sentence = baseEntry.Examples[index];

            return new OrderPartsRepetitionTask(baseEntry.UserId, baseEntry.Id, sentence);
        }

        public YesOrNoRepetitionTask CreateYesOrNoTask(bool isForward, VocabularyEntry baseEntry, VocabularyEntry entryForOption)
        {
            string prompt = isForward ? baseEntry.Foreign : baseEntry.Translations[0];
            string option;

            bool isCorrect = Random.Shared.Next(2) == 0;
            if (isCorrect)
                option = isForward ? baseEntry.Translations[0] : baseEntry.Foreign;
            else
                option = isForward ? entryForOption.Translations[0] : entryForOption.Foreign;

            prompt = $"{prompt} -> {option}";

            return new YesOrNoRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, isCorrect);
        }
    }
}
