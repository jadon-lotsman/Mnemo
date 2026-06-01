namespace Mnemo.Contracts.Dtos.Repetition.Responses
{
    public class RepetitionTaskResponse
    {
        public int Id { get; set; }
        public string? Prompt { get; set; }
        public string[]? Options { get; set; }
    }
}
