using Mnemo.Shared.Enums;

namespace Mnemo.Entities
{
    public class Glossary
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public Guid? SharingGuid { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public List<GlossaryLink> Links { get; set; }


        public Glossary()
        {
            Guid = Guid.NewGuid();
            SharingGuid = null;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;

            Links = new List<GlossaryLink>();
        }
    }
}
