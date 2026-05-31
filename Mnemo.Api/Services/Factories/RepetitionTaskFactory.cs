using System;
using System.Threading.Tasks;
using Mnemo.Data.Entities;
using Mnemo.Services.Queries;

namespace Mnemo.Services.Factories
{
    public class RepetitionTaskFactory
    {
        private static Random _random = new Random();

        public RepetitionTask Create(VocabularyEntry baseEntry, List<VocabularyEntry> entriesForOptions)
        {
            bool isForwardQuestion = _random.Next(2) == 0;
            bool withOptions = _random.Next(2) == 0;

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

            return new RepetitionTask(baseEntry.Id, isForwardQuestion, prompt, options);
        }
    }
}
