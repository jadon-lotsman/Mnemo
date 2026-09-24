namespace Mnemo.Data.Entities
{
    public class VocabularyEntryLink
    {
        public int VocabularyId { get; set; }
        public Vocabulary Vocabulary { get; set; }

        public int VocabularyEntryId { get; set; }
        public VocabularyEntry VocabularyEntry { get; set; }

        public DateTime AddedAt { get; set; }


        public VocabularyEntryLink()
        {
            AddedAt = DateTime.UtcNow;
        }
    }
}
