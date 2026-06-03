namespace Mnemo.Contracts.Vocabulary.Requests
{
    public class PatchEntryRequest
    {
        public string? Foreign { get; set; }
        public string? Transcription { get; set; }
        public string[]? ExamplesAdd { get; set; }
        public string[]? ExamplesRemove { get; set; }
        public string[]? TranslationsAdd { get; set; }
        public string[]? TranslationsRemove { get; set; }
    }
}
