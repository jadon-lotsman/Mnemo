using System.Threading.Tasks;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Data.Entities
{
    public class RepetitionTask
    {
        public int Id { get; set; }
        public int AsessmentEntryId { get; set; }

        public string Prompt { get; set; }
        public List<string> Options { get; set; }
        public List<string> CorrectAnswers { get; set; }
        public string UserAnswer { get; set; }
        public int ActionCounter { get; set; }
        public TimeSpan ElapsedTime { get; set; }


        public int UserId { get; set; }
        public User User { get; set; }


        public RepetitionTask() { }

        public RepetitionTask(string prompt, List<string> correctAnswers, List<string> options, int userId, int entryId)
        {
            Prompt = prompt;
            Options = options;
            CorrectAnswers = correctAnswers;
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

        public double ComputeQuality(TimeSpan averageTime)
        {
            double similarity = CorrectAnswers.Max(UserAnswer.ComputeLevenshteinSimilarity);

            return SM2Helper.ComputeQuality(averageTime, ElapsedTime, ActionCounter, similarity);
        }
    }
}
