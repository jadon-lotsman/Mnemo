using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Data.Entities
{
    public abstract class RepetitionTask
    {
        public int Id { get; set; }
        public int VocabularyEntryId { get; set; }
        public PartOfSpeech? EntryPartOfSpeech { get; set; }

        public string Prompt { get; set; }
        public string UserAnswer { get; set; }
        public int OrderIndex { get; set; }
        public int ActionCounter { get; set; }
        public TimeSpan ElapsedTime { get; set; }


        public int UserId { get; set; }
        public User User { get; set; }


        public RepetitionTask() { }

        public RepetitionTask(string prompt, int userId, int entryId, PartOfSpeech? partOfSpeech = null)
        {
            Prompt = prompt;
            UserAnswer = string.Empty;
            EntryPartOfSpeech = partOfSpeech;

            UserId = userId;
            VocabularyEntryId = entryId;
        }


        public void SubmitAnswer(string userAnswer, TimeSpan elapsedTime)
        {
            ActionCounter++;
            UserAnswer = userAnswer;

            if (ElapsedTime == TimeSpan.Zero)
                ElapsedTime = elapsedTime;
        }


        protected abstract double GetSimilarity();

        public abstract string GetCorrect();

        public double GetQuality(TimeSpan averageTime)
        {
            double similarity = GetSimilarity();

            return SM2Helper.ComputeQuality(averageTime, ElapsedTime, ActionCounter, similarity);
        }
    }

    public class TextRepetitionTask : RepetitionTask
    {
        public List<string> CorrectAnswers { get; set; }


        public TextRepetitionTask() { }

        public TextRepetitionTask(string prompt, PartOfSpeech? partOfSpeech, int userId, int entryId, List<string> correctAnswers) : base(prompt, userId, entryId, partOfSpeech)
        {
            CorrectAnswers = correctAnswers;
        }

        protected override double GetSimilarity()
        {
            return CorrectAnswers.Max(UserAnswer.ComputeLevenshteinSimilarity);
        }

        public override string GetCorrect() => CorrectAnswers.First();
    }

    public class OptionRepetitionTask : RepetitionTask
    {
        public List<string> Options { get; set; }
        public string CorrectOption { get; set; }


        public OptionRepetitionTask() { }

        public OptionRepetitionTask(string prompt, PartOfSpeech? partOfSpeech, int userId, int entryId, List<string> options, string correctOption) : base(prompt, userId, entryId, partOfSpeech)
        {
            if (!options.Contains(correctOption))
                options.Add(correctOption);

            Options = options
                .OrderBy(x => Random.Shared.Next())
                .ToList();

            CorrectOption = correctOption;
        }

        protected override double GetSimilarity()
        {
            return CorrectOption == UserAnswer ? 1.0 : 0.0;
        }

        public override string GetCorrect() => CorrectOption;
    }

    public class SentenceReorderRepetitionTask : RepetitionTask
    {
        public List<string> SentenceParts { get; set; }
        public string CorrectOrder { get; set; }


        public SentenceReorderRepetitionTask() { }

        public SentenceReorderRepetitionTask(int userId, int entryId, string sentence) : base("", userId, entryId)
        {
            CorrectOrder = TextNormalizer.NormalizeExample(sentence);

            var parts = CorrectOrder.Split().ToList();

            int mergeCount = (int)Math.Floor(parts.Count / 5d);

            while (mergeCount > 0 || parts.Count > 10) 
            {
                if (parts.Count >= 2)
                {
                    int index = Random.Shared.Next(parts.Count - 1);
                    parts[index] = string.Join(' ', parts[index], parts[index + 1]);
                    parts.RemoveAt(index + 1);

                    mergeCount--;
                }
                else
                {
                    break;
                }
            }

            SentenceParts = parts
                .OrderBy(x => Random.Shared.Next())
                .ToList();
        }

        protected override double GetSimilarity()
        {
            return UserAnswer.AddEndPointIfNeeded().RemoveSpaces() == CorrectOrder.RemoveSpaces() ? 1.0 : 0.0;
        }

        public override string GetCorrect() => CorrectOrder;
    }

    public class SyllableReorderRepetitionTask : RepetitionTask
    {
        public List<string> Syllables { get; set; }
        public string CorrectOrder { get; set; }


        public SyllableReorderRepetitionTask() { }

        public SyllableReorderRepetitionTask(PartOfSpeech? partOfSpeech, int userId, int entryId, string word, List<string> distractors) : base("", userId, entryId, partOfSpeech)
        {
            CorrectOrder = word.ToLower();

            Syllables = CorrectOrder
                .SplitIntoChunks(3)
                .Union(distractors)
                .OrderBy(x => Random.Shared.Next())
                .ToList();
        }

        protected override double GetSimilarity()
        {
            return UserAnswer.RemoveSpaces() == CorrectOrder.RemoveSpaces() ? 1.0 : 0.0;
        }

        public override string GetCorrect() => CorrectOrder;
    }

    public class YesOrNoRepetitionTask : RepetitionTask
    {
        public string Option { get; set; }
        public bool CorrectYesOrNo { get; set; }


        public YesOrNoRepetitionTask() { }

        public YesOrNoRepetitionTask(string prompt, int userId, int entryId, string option, bool correctYesOrNo) : base(prompt, userId, entryId)
        {
            Option = option;
            CorrectYesOrNo = correctYesOrNo;
        }

        protected override double GetSimilarity()
        {
            bool isCorrect = UserAnswer switch
            {
                "да" or "yes" or "true" => CorrectYesOrNo,
                "нет" or "no" or "false" => !CorrectYesOrNo,
                _ => false
            };

            return isCorrect ? 1.0 : 0.0;
        }

        public override string GetCorrect() => CorrectYesOrNo ? "yes" : "no";
    }
}
