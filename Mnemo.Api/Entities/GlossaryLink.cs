namespace Mnemo.Entities
{
    public class GlossaryLink
    {
        public int GlossaryId { get; set; }
        public Glossary Glossary { get; set; }

        public int EntryId { get; set; }
        public GlossaryEntry Entry { get; set; }

        public DateTime AddedAt { get; set; }


        public GlossaryLink()
        {
            AddedAt = DateTime.UtcNow;
        }
    }
}
