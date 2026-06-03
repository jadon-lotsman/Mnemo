namespace Mnemo.Contracts.Vocabulary
{
    public class EntryResponse
    {
        public int Id { get; set; }
        public string? Foreign { get; set; }
        public string? Transcription { get; set; }
        public string[]? Examples { get; set; }
        public string[]? Translations { get; set; }
    }
}
