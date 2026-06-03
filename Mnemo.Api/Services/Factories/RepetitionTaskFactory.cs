using Mnemo.Data.Entities;

namespace Mnemo.Services.Factories
{
    public class RepetitionTaskFactory
    {
        public RepetitionTask Create(VocabularyEntry baseEntry, List<VocabularyEntry> entriesForOptions)
        {
            bool isForwardQuestion = Random.Shared.Next(2) == 0;
            bool withOptions = entriesForOptions.Count >= 2 ? Random.Shared.Next(2) == 0 : false;

            string prompt;
            var correctAnswers = new List<string>();
            if (isForwardQuestion)
            {
                correctAnswers = baseEntry.Translations.ToList();
                prompt = baseEntry.Foreign;
            }
            else
            {
                correctAnswers.Add(baseEntry.Foreign);
                prompt = baseEntry.Translations[0];
            }

            var options = new List<string>();
            if (withOptions)
            {
                foreach (var entry in entriesForOptions)
                {
                    string option = isForwardQuestion ? entry.Translations[0] : entry.Foreign;
                    if (!options.Contains(option) && !correctAnswers.Contains(option))
                        options.Add(option);
                }

                return new OptionRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, options, correctAnswers[0]);
            }

            return new TextRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, correctAnswers);
        }
    }
}
