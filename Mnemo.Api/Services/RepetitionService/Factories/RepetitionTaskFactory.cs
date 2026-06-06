using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Mnemo.Data.Entities;

namespace Mnemo.Services.RepetitionService.Factories
{
    public class RepetitionTaskFactory
    {
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

        public YesOrNoRepetitionTask CreateYesOrNoTask(VocabularyEntry baseEntry, VocabularyEntry entryForOption)
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
                int index = Random.Shared.Next(entryForOption.Translations.Count);
                option = entryForOption.Translations[index];
            }

            return new YesOrNoRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, option, isCorrect);
        }
    }
}
