using Mnemo.Shared.SM2Helper;

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
            RepetitionCounter = 0;
            RepetitionInterval = SM2Helper.MinInterval;
            EasinessFactor = SM2Helper.InitEF;
            LastRepetitionAt = DateOnly.MinValue;
            NextRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
        }


        public bool TryRecordQuality(double quality, DateOnly today, out string? errorMessage)
        {
            if (quality < SM2Helper.MinQuality || quality > SM2Helper.MaxQuality)
            {
                errorMessage = $"Quality {Math.Round(quality, 1)} out of range {SM2Helper.MinQuality}...{SM2Helper.MaxQuality}";
                return false;
            }


            bool isPassing = SM2Helper.IsPassingQuality(quality);

            if (IsDueAt(today))
            {
                RepetitionCounter = isPassing ? RepetitionCounter + 1 : 0;
                LastRepetitionAt = today;


                (int interval, double easinessFactor)
                    = SM2Helper.NextIntervalAndEf(EasinessFactor, RepetitionInterval, RepetitionCounter, quality);

                RepetitionInterval = interval;
                EasinessFactor = easinessFactor;
                NextRepetitionAt = LastRepetitionAt.AddDays(interval);
            }
            else
            {
                if (isPassing)
                    EasinessFactor = EasinessFactor + SM2Helper.OvertimeBonusEF;

                LastRepetitionAt = today;
            }


            errorMessage = null;
            return true;
        }

        public bool IsDueAt(DateOnly date) => NextRepetitionAt <= date;
    }
}
