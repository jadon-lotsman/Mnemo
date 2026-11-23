namespace Mnemo.Contracts.Repetition
{
    public class RepetitionDateResponse
    {
        public DateOnly Date { get; set; }
        public bool IsImportantDay { get; set; }
        public string[]? Foreigns { get; set; }
    }
}
