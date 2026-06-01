using Mnemo.Shared;

namespace Mnemo.Data.Entities
{
    public class RepetitionState
    {
        public int Id { get; set; }

        public int RepetitionCounter { get; set; }
        public int RepetitionInterval { get; set; }
        public double EasinessFactor { get; set; }
        public bool CanSelfAssess { get; set; }
        public DateOnly LastRepetitionAt { get; set; }
        public DateOnly NextRepetitionAt { get; set; }


        public int UserId { get; set; }
        public User User { get; set; }
        public int VocabularyEntryId { get; set; }
        public VocabularyEntry VocabularyEntry { get; set; }


        public RepetitionState() { }

        public RepetitionState(int userId, VocabularyEntry entry)
        {
            RepetitionCounter = 0;
            RepetitionInterval = SM2Helper.MinInterval;
            EasinessFactor = SM2Helper.InitEF;
            CanSelfAssess = false;
            LastRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
            NextRepetitionAt = LastRepetitionAt.AddDays(SM2Helper.MinInterval);

            UserId = userId;
            VocabularyEntry = entry;
        }
    }
}
