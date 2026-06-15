namespace Mnemo.Contracts.Vocabulary
{
    public class EnrichResponse
    {
        public string? Transcription { get; set; }
        public string? TranscriptionAudioUrl { get; set; }
        public string[]? Synonyms { get; set; }
        public string[]? Antonyms { get; set; }
    }
}
