using Mnemo.Contracts.Vocabulary;

namespace Mnemo.Contracts.Repetition
{
    public class RepetitionResultResponse
    {
        public int Correct { get; set; }
        public int Total { get; set; }
        public int Percent { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public EntryResponse[]? VocabularyEntries { get; set; }
    }
}
