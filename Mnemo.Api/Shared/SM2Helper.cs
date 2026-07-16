namespace Mnemo.Shared
{
    public static class SM2Helper
    {
        public const double MinEF = 1.3;
        public const double InitEF = 2.5;
        public const double ImportantDayEF = 1.8;
        public const double OvertimeBonusEF = 0.05;

        public const int MinInterval = 1;
        public const int MaxInterval = 365;

        public const int MinQuality = 0;
        public const int MaxQuality = 5;

        private const int FirstIntervalDays = 1;
        private const int SecondIntervalDays = 3;


        public static double ComputeQuality(TimeSpan averageTime, TimeSpan actionTime, int actionCounter, double similarity)
        {
            if (similarity <= 0.88) return MinQuality;
            if (similarity >= 1.0) return MaxQuality;

            var changeCounter = Math.Max(0, actionCounter - 1);
            double Stability = Math.Exp(-changeCounter * 0.5f);

            double Accuracy = CalcSigmoidAccuracy(similarity);

            var ratio = actionTime > TimeSpan.Zero ? averageTime / actionTime : 0;
            double Reaction = CalcPowReaction(ratio);

            double Knowledge = 0.6 * Accuracy + 0.2 * Stability + 0.2 * Reaction;

            double Quality = Math.Clamp(Knowledge, 0, 1) * MaxQuality;

            return Math.Round(Quality, 1);
        }


        public static (int newInterval, double newEasinessFactor) NextIntervalAndEf(double easinessFactor, int interval, int repetitionCounter, double quality)
        {
            double nextEasinessFactor;
            int nextInterval;

            if (!IsPassingQuality(quality))
            {
                nextInterval = FirstIntervalDays;
            }
            else
            {
                nextInterval = repetitionCounter switch
                {
                    0 => FirstIntervalDays,
                    1 => SecondIntervalDays,
                    _ => (int) Math.Ceiling((interval > 0 ? interval : 1) * easinessFactor)
                };

                nextInterval = Math.Clamp(nextInterval, MinInterval, MaxInterval);
            }

            nextEasinessFactor = easinessFactor + (0.1 - (MaxQuality - quality) * (0.08 + (MaxQuality - quality) * 0.02));
            nextEasinessFactor = Math.Max(nextEasinessFactor, MinEF);

            return (nextInterval, nextEasinessFactor);
        }


        public static bool IsPassingQuality(double quality) => quality >= 3;

        private static double CalcSigmoidAccuracy(double similarity, double center = 0.8, double steepness = 10.0)
        {
            return 1.0 / (1.0 + Math.Exp(-steepness * (similarity - center)));
        }

        private static double CalcPowReaction(double ratio, double min = 0.7, double max = 1.2, double center = 1.0, double steepness = 1.5)
        {
            double raw = Math.Pow(ratio, steepness);
            return Math.Clamp(raw, min, max);
        }
    }
}
