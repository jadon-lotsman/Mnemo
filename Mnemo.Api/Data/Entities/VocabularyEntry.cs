using Mnemo.Shared;

namespace Mnemo.Data.Entities
{
    public class VocabularyEntry
    {
        public int Id { get; set; }

        public PartOfSpeech? PartOfSpeech { get; set; }
        public string Foreign { get; set; }
        public string? Transcription { get; set; }
        public string? TranscriptionAudioUrl { get; set; }
        public List<string> Examples { get; set; }
        public List<string> Translations { get; set; }
        public List<string> Synonyms { get; set; }
        public List<string> Antonyms { get; set; }
        public EnrichmentStatus EnrichmentStatus { get; set; }
        public DateTime LastEnrichmentAt { get; set; }
        public DateTime CreatedAt { get; set; }


        public int UserId { get; set; }
        public User User { get; set; }
        public RepetitionState? RepetitionState { get; set; }


        public VocabularyEntry()
        {
            Examples = new List<string>();
            Translations = new List<string>();
            Synonyms = new List<string>();
            Antonyms = new List<string>();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
