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

        public RepetitionState(int userId, int entryId)
        {
            RepetitionCounter = 0;
            RepetitionInterval = SM2Helper.MinInterval;
            EasinessFactor = SM2Helper.InitEF;
            CanSelfAssess = false;
            LastRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
            NextRepetitionAt = LastRepetitionAt.AddDays(RepetitionInterval);

            UserId = userId;
            VocabularyEntryId = entryId;
        }


        public bool TryRecordQuality(double quality, bool isSelfAssess, DateOnly today, out string? errorMessage)
        {
            if (isSelfAssess && !CanSelfAssess)
            {
                errorMessage = "Self-assessment not allowed";
                return false;
            }
            else if (quality < SM2Helper.MinQuality || quality > SM2Helper.MaxQuality)
            {
                errorMessage = $"Quality {Math.Round(quality, 1)} out of range {SM2Helper.MinQuality}...{SM2Helper.MaxQuality}";
                return false;
            }


            if (IsDueAt(today))
            {
                if (isSelfAssess)
                {
                    CanSelfAssess = false;
                }
                else
                {
                    bool isPassing = SM2Helper.IsPassingQuality(quality);
                    RepetitionCounter = isPassing ? RepetitionCounter + 1 : 0;
                    CanSelfAssess = isPassing;
                    LastRepetitionAt = today;
                }


                (int interval, double easinessFactor)
                    = SM2Helper.NextIntervalAndEf(EasinessFactor, RepetitionInterval, RepetitionCounter, quality);

                RepetitionInterval = interval;
                EasinessFactor = easinessFactor;
                NextRepetitionAt = LastRepetitionAt.AddDays(interval);
            }
            else
            {
                LastRepetitionAt = today;
            }


            errorMessage = null;
            return true;
        }

        public bool IsDueAt(DateOnly date) => NextRepetitionAt <= date;
    }
}
