namespace Mnemo.Entities
{
    public class GlossaryLink
    {
        public Guid GlossaryId { get; set; }
        public Glossary Glossary { get; set; }

        public Guid EntryId { get; set; }
        public GlossaryEntry Entry { get; set; }

        public DateTime AddedAt { get; set; }


        public GlossaryLink()
        {
            AddedAt = DateTime.UtcNow;
        }
    }
}
