using Mnemo.Data.Entities;

namespace Mnemo.Services.Factories
{
    public class RepetitionTaskFactory
    {
        public RepetitionTask Create(VocabularyEntry baseEntry, List<VocabularyEntry> entriesForOptions)
        {
            bool isForwardQuestion = Random.Shared.Next(2) == 0;
            bool withOptions = entriesForOptions.Count >= 2 ? Random.Shared.Next(2) == 0 : false;

            string prompt, answer;
            if (isForwardQuestion)
            {
                answer = baseEntry.Translations[0];
                prompt = baseEntry.Foreign;
            }
            else
            {
                answer = baseEntry.Foreign;
                prompt = baseEntry.Translations[0];
            }

            var options = new List<string>();
            if (withOptions)
            {
                foreach (var entry in entriesForOptions)
                {
                    string option = isForwardQuestion ? entry.Translations[0] : entry.Foreign;
                    if (!options.Contains(option) && option != answer)
                        options.Add(option);
                }

                options.Add(answer);
                options = options.OrderBy(x => Guid.NewGuid()).ToList();
            }

            return new RepetitionTask(isForwardQuestion, prompt, options, baseEntry.Id);
        }
    }
}
