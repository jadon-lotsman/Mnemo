namespace Mnemo.Contracts.Vocabulary.Requests
{
    public class CreateEntryRequest
    {
        public string? Foreign { get; set; }
        public string? Transcription { get; set; }
        public string[]? Examples { get; set; }
        public string[]? Translations { get; set; }
    }
}
