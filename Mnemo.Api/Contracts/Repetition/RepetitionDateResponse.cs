namespace Mnemo.Contracts.Repetition
{
    public class RepetitionDateResponse
    {
        public DateOnly Date { get; set; }
        public bool IsImportantDay { get; set; }
        public string[]? VocabularyForeigns { get; set; }
    }
}
