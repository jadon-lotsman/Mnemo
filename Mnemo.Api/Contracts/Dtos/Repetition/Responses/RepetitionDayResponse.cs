namespace Mnemo.Contracts.Dtos.Repetition.Responses
{
    public class RepetitionDayResponse
    {
        public DateOnly Date { get; set; }
        public string[]? VocabularyForeigns { get; set; }
    }
}
