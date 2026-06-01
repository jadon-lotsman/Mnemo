using System.Threading.Tasks;
using Mnemo.Shared;
using Mnemo.Shared.Extensions;

namespace Mnemo.Data.Entities
{
    public class RepetitionTask
    {
        public int Id { get; set; }

        public string Prompt { get; set; }
        public List<string> Options { get; set; }
        public string UserAnswer { get; set; }
        public bool IsForwardQuestion { get; set; }
        public int ActionCounter { get; set; }
        public TimeSpan ActionTimeSpan { get; set; }


        public int RepetitionSessionId { get; set; }
        public RepetitionSession RepetitionSession { get; set; }
        public int VocabularyEntryId { get; set; }
        public VocabularyEntry VocabularyEntry { get; set; }


        public RepetitionTask() { }

        public RepetitionTask(bool isForwardQuestion, string prompt, List<string> options, int entryId)
        {
            IsForwardQuestion = isForwardQuestion;
            Prompt = prompt;
            Options = options;
            UserAnswer = string.Empty;

            VocabularyEntryId = entryId;
        }


        public void SubmitAnswer(string userAnswer, DateTime currentTime)
        {
            ActionCounter++;
            UserAnswer = userAnswer;
            ActionTimeSpan = currentTime - RepetitionSession.LastActionAt;
            RepetitionSession.LastActionAt = currentTime;
        }

        public double ComputeQuality()
        {
            double similarity;

            if (IsForwardQuestion)
                similarity = VocabularyEntry.Translations.Max(UserAnswer.ComputeLevenshteinSimilarity);
            else
                similarity = UserAnswer.ComputeLevenshteinSimilarity(VocabularyEntry.Foreign);

            return SM2Helper.ComputeQuality(RepetitionSession.AverageActionTime, ActionTimeSpan, ActionCounter, similarity);
        }
    }
}
