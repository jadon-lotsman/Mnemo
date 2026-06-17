using Mnemo.Contracts.Vocabulary;
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
            EnrichmentStatus = EnrichmentStatus.Pending;
            LastEnrichmentAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }


        public bool EnrichMeta(EnrichResponse enrich)
        {
            if (enrich == null)
                return false;

            bool isEnriched = false;

            if (Transcription == null && enrich.Transcription != null)
            {
                Transcription = enrich.Transcription;

                if (TranscriptionAudioUrl == null && enrich.TranscriptionAudioUrl != null)
                    TranscriptionAudioUrl = enrich.TranscriptionAudioUrl;

                isEnriched = true;
            }

            if (enrich.Synonyms?.Any() == true)
            {
                Synonyms = enrich.Synonyms.ToList();
                isEnriched = true;

            }

            if (enrich.Antonyms?.Any() == true)
            {
                Antonyms = enrich.Antonyms.ToList();
                isEnriched = true;
            }

            return isEnriched;
        }

        public void ResetAllMeta()
        {
            Transcription = null;
            TranscriptionAudioUrl = null;
            Synonyms.Clear();
            Antonyms.Clear();
            EnrichmentStatus = EnrichmentStatus.Pending;
        }

        public void ResetAudio()
        {
            TranscriptionAudioUrl = null;
            EnrichmentStatus = EnrichmentStatus.Pending;
        }
    }
}
