namespace Mnemo.Contracts.Dtos.Vocabulary.Responses
{
    public class VocabularyEntryResponse
    {
        public int Id { get; set; }
        public string? Foreign { get; set; }
        public string? Transcription { get; set; }
        public string[]? Examples { get; set; }
        public string[]? Translations { get; set; }
    }
}
