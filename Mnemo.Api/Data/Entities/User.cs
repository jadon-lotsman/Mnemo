namespace Mnemo.Data.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public DateTime RegisteredAt { get; set; }


        public List<Glossary> Glossaries { get; set; }
        public List<RepetitionTask> RepetitionTasks { get; set; }


        public User()
        {
            RegisteredAt = DateTime.UtcNow;

            Glossaries = new List<Glossary>();
            RepetitionTasks = new List<RepetitionTask>();
        }
    }
}
