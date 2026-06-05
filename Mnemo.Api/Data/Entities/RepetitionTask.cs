using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Data.Entities
{
    public abstract class RepetitionTask
    {
        public int Id { get; set; }
        public int AsessmentEntryId { get; set; }

        public string Prompt { get; set; }
        public string UserAnswer { get; set; }
        public int ActionCounter { get; set; }
        public TimeSpan ElapsedTime { get; set; }


        public int UserId { get; set; }
        public User User { get; set; }


        public RepetitionTask() { }

        public RepetitionTask(string prompt, int userId, int entryId)
        {
            Prompt = prompt;
            UserAnswer = string.Empty;

            UserId = userId;
            AsessmentEntryId = entryId;
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

        public TextRepetitionTask(string prompt, int userId, int entryId, List<string> correctAnswers) : base(prompt, userId, entryId)
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

        public OptionRepetitionTask(string prompt, int userId, int entryId, List<string> options, string correctOption) : base(prompt, userId, entryId)
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

    public class OrderPartsRepetitionTask : RepetitionTask
    {
        public List<string> SentenceParts { get; set; }
        public string CorrectOrder { get; set; }


        public OrderPartsRepetitionTask() { }

        public OrderPartsRepetitionTask(int userId, int entryId, string sentence) : base("", userId, entryId)
        {
            SentenceParts = sentence.RemoveMultispaces()
                .Split(" ")
                .OrderBy(x => Random.Shared.Next())
                .ToList();

            CorrectOrder = sentence;
        }

        protected override double GetSimilarity()
        {
            return UserAnswer == CorrectOrder ? 1.0 : 0.0;
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
