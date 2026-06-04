using Mnemo.Shared;

namespace Mnemo.Data.Entities
{
    public class RepetitionState
    {
        public int Id { get; set; }

        public int RepetitionCounter { get; set; }
        public int RepetitionInterval { get; set; }
        public double EasinessFactor { get; set; }
        public bool CanAdjustToday { get; set; }
        public DateOnly LastRepetitionAt { get; set; }
        public DateOnly NextRepetitionAt { get; set; }


        public int VocabularyEntryId { get; set; }
        public VocabularyEntry VocabularyEntry { get; set; }


        public RepetitionState()
        {
            RepetitionCounter = 0;
            RepetitionInterval = SM2Helper.MinInterval;
            EasinessFactor = SM2Helper.InitEF;
            CanAdjustToday = false;
            LastRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
            NextRepetitionAt = DateOnly.FromDateTime(DateTime.UtcNow);
        }


        public bool TryRecordQuality(double quality, bool isAdjust, DateOnly today, out string? errorMessage)
        {
            if (isAdjust && !CanAdjustToday)
            {
                errorMessage = "Adjustment is only available today after a successful repetition";
                return false;
            }
            else if (quality < SM2Helper.MinQuality || quality > SM2Helper.MaxQuality)
            {
                errorMessage = $"Quality {Math.Round(quality, 1)} out of range {SM2Helper.MinQuality}...{SM2Helper.MaxQuality}";
                return false;
            }


            if (IsDueAt(today))
            {
                if (isAdjust)
                {
                    CanAdjustToday = false;
                }
                else
                {
                    bool isPassing = SM2Helper.IsPassingQuality(quality);
                    RepetitionCounter = isPassing ? RepetitionCounter + 1 : 0;
                    CanAdjustToday = isPassing;
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
