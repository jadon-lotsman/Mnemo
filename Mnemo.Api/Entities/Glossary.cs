namespace Mnemo.Entities
{
    public class Glossary
    {
        public Guid Id { get; set; }
        public Guid? ShareGuid { get; set; } = null;
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public List<GlossaryLink> Links { get; set; } = [];


        public Glossary()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }
    }
}
