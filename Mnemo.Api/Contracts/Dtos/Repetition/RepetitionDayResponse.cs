using Mnemo.Contracts.Dtos.Vocabulary;

namespace Mnemo.Contracts.Dtos.Repetition
{
    public class RepetitionDayResponse
    {
        public DateOnly Date { get; set; }
        public string[]? VocabularyForeigns { get; set; }
    }
}
