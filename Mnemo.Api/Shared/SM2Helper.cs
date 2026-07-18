namespace Mnemo.Shared
{
    public static class SM2Helper
    {
        public const double MinEF = 1.3;
        public const double InitEF = 2.5;
        public const double MaxEF = 3.2;
        public const double ImportantDayEF = 1.8;
        public const double OvertimeBonusEF = 0.05;

        public const int MinInterval = 1;
        public const int MaxInterval = 365;

        public const int MinQuality = 0;
        public const int MaxQuality = 5;
        public const int PassingQuality = 3;

        private const int FirstIntervalDays = 1;
        private const int SecondIntervalDays = 3;


        public static double ComputeRecallQuality(TimeSpan averageTime, TimeSpan actionTime, int actionCounter, double similarity, double difficulty)
        {
            double penalty = similarity < 0.99d ? Math.Pow(similarity, 2.0d) : 1;
            double Accuracy = CalcAccuracySigmoid(similarity * penalty);

            double Stability = CalcStabilityExp(actionCounter);

            var timeRatio = CalcReactionRatio(actionTime, averageTime);
            double Reaction = CalcReactionPow(timeRatio);

            double rawKnowledge = (0.3d * Accuracy + 0.4d * Stability + 0.3d * Reaction) * difficulty;
            double Quality = rawKnowledge * MaxQuality;

            if (Accuracy < 0.5d)
                return MinQuality;
            else if (!IsPassingQuality(Quality) && Accuracy >= 0.8d)
                return PassingQuality;

            Quality = Math.Clamp(Quality, 0, MaxQuality);
            Quality = Math.Round(Quality, 1);


            return Quality;
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
                    _ => (int)Math.Ceiling((interval > 0 ? interval : 1) * easinessFactor)
                };

                nextInterval = Math.Clamp(nextInterval, MinInterval, MaxInterval);
            }

            nextEasinessFactor = easinessFactor + (0.1 - (MaxQuality - quality) * (0.08 + (MaxQuality - quality) * 0.02));
            nextEasinessFactor = Math.Clamp(nextEasinessFactor, MinEF, MaxEF);

            return (nextInterval, nextEasinessFactor);
        }


        public static bool IsPassingQuality(double quality) => quality >= PassingQuality;

        private static double CalcStabilityExp(int actionCounter)
        {
            var changeCounter = Math.Max(0, actionCounter - 1);
            return Math.Exp(-changeCounter * 0.5f);
        }

        private static double CalcAccuracySigmoid(double similarity, double center = 0.5, double steepness = 8.0)
        {
            return 1.0 / (1.0 + Math.Exp(-steepness * (similarity - center)));
        }

        private static double CalcReactionRatio(TimeSpan actionTime, TimeSpan averageTime)
        {
            double actionSec = Math.Max(actionTime.TotalSeconds, 0.5);
            double avgSec = Math.Max(averageTime.TotalSeconds, 0.5);

            double ratio = Math.Log(avgSec + 1) / Math.Log(actionSec + 1);
            return ratio;
        }

        private static double CalcReactionPow(double ratio, double min = 0.7, double max = 1.1, double steepness = 0.75)
        {
            double raw = Math.Pow(ratio, steepness);
            return Math.Clamp(raw, min, max);
        }
    }
}
