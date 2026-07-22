namespace Mnemo.Data.Entities
{
    public class RepetitionState
    {
        public int Id { get; set; }

        public int RepetitionCounter { get; set; }
        public int RepetitionInterval { get; set; }
        public double EasinessFactor { get; set; }
        public DateOnly LastRepetitionAt { get; set; }
        public DateOnly NextRepetitionAt { get; set; }


        public int VocabularyEntryId { get; set; }
        public VocabularyEntry VocabularyEntry { get; set; }


        public RepetitionState()
        {
            LastRepetitionAt = DateOnly.MinValue;
            NextRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
        }


        public void SetState(int interval, double easinessFactor, bool isPassing, DateOnly today)
        {
            RepetitionCounter = isPassing ? RepetitionCounter + 1 : 0;
            LastRepetitionAt = today;

            RepetitionInterval = interval;
            EasinessFactor = easinessFactor;
            NextRepetitionAt = LastRepetitionAt.AddDays(interval);
        }

        public void SetBonus(double easinessBonusEF, bool isPassing, DateOnly today)
        {
            if (isPassing)
                EasinessFactor += easinessBonusEF;

            LastRepetitionAt = today;
        }

        public bool IsDueAt(DateOnly date) => NextRepetitionAt <= date;
    }
}
