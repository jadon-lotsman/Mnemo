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

        public async Task<RepetitionTask> CreateByTypeAsync(bool isForward, Type taskType, VocabularyEntry baseEntry, params int[] excludeIds)
        {
            if (taskType == typeof(OptionRepetitionTask))
            {
                var distractors = await _provider.GetDistractorsAsync(isForward, baseEntry.UserId, 3, excludeIds);
                if (!distractors.Any())
                    return CreateTextTask(isForward, baseEntry);

                return CreateOptionsTask(isForward, baseEntry, distractors);
            }
            else if (taskType == typeof(SentenceReorderRepetitionTask))
            {
                return CreateSentenceReorderTask(baseEntry);
            }
            else if (taskType == typeof(SyllableReorderRepetitionTask) && baseEntry.Foreign.Length >= 8)
            {
                return CreateSyllableReorderTask(baseEntry);
            }
            else if (taskType == typeof(YesOrNoRepetitionTask))
            {
                var distractor = await _provider.GetDistractorsAsync(isForward, baseEntry.UserId, 1, excludeIds);
                if (!distractor.Any())
                    return CreateTextTask(isForward, baseEntry);

                return CreateYesOrNoTask(isForward, baseEntry, distractor[0]);
            }

            return CreateTextTask(isForward, baseEntry);
        }

        public TextRepetitionTask CreateTextTask(bool isForward, VocabularyEntry baseEntry)
        {
            string prompt = isForward ? baseEntry.Foreign : baseEntry.Translations[0];
            var correct = isForward ? baseEntry.Translations : [baseEntry.Foreign];

            return new TextRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, correct);
        }

        public OptionRepetitionTask CreateOptionsTask(bool isForward, VocabularyEntry baseEntry, List<string> distractors)
        {
            string prompt = isForward ? baseEntry.Foreign : baseEntry.Translations[0];
            var correct = isForward ? baseEntry.Translations[0] : baseEntry.Foreign;

            return new OptionRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, distractors, correct);
        }

        public SentenceReorderRepetitionTask CreateSentenceReorderTask(VocabularyEntry baseEntry)
        {
            int index = Random.Shared.Next(baseEntry.Examples.Count);
            var sentence = baseEntry.Examples[index];

            return new SentenceReorderRepetitionTask(baseEntry.UserId, baseEntry.Id, sentence);
        }

        public SyllableReorderRepetitionTask CreateSyllableReorderTask(VocabularyEntry baseEntry)
        {
            var foreign = baseEntry.Foreign;

            return new SyllableReorderRepetitionTask(baseEntry.UserId, baseEntry.Id, foreign);
        }

        public YesOrNoRepetitionTask CreateYesOrNoTask(bool isForward, VocabularyEntry baseEntry, string distractor)
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
                option = distractor;
            }

            return new YesOrNoRepetitionTask(prompt, baseEntry.UserId, baseEntry.Id, option, isCorrect);
        }
    }
}
