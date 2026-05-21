using Mnemo.Contracts.Dtos.Vocabulary;

namespace Mnemo.Data.Entities
{
    public class RepetitionResult
    {
        public int Id { get; set; }
        public int Correct { get; set; }
        public int Total { get; set; }
        public int Percent { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public List<VocabularyEntry> VocabularyEntries { get; set; }


        public RepetitionResult() { }

        public RepetitionResult(int correct, List<VocabularyEntry> entries)
        {
            Correct = correct;
            Total = entries.Count;

            float percent = Total == 0 ? 0 : (float) Correct / Total * 100;
            Percent = (int) Math.Round(percent);

            VocabularyEntries = entries;
        }
    }
}
