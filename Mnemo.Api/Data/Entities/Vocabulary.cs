using Mnemo.Shared.Enums;

namespace Mnemo.Data.Entities
{
    public class Vocabulary
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid? SharingGuid { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public List<VocabularyEntryLink> EntryLinks { get; set; }


        public Vocabulary()
        {
            Guid = Guid.NewGuid();
            SharingGuid = null;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;

            EntryLinks = new List<VocabularyEntryLink>();
        }
    }
}
