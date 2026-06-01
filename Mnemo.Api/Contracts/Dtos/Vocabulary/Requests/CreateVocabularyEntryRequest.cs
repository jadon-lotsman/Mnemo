namespace Mnemo.Contracts.Dtos.Vocabulary.Requests
{
    public class CreateVocabularyEntryRequest
    {
        public string? Foreign { get; set; }
        public string? Transcription { get; set; }
        public string[]? Examples { get; set; }
        public string[]? Translations { get; set; }
    }
}
