namespace Mnemo.Shared.SM2Helper
{
    public static class SM2Helper
    {
        public const double MinEF = 1.3;
        public const double InitEF = 2.5;
        public const double MaxEF = 3.2;
        public const double OvertimeBonusEF = 0.05;

        public const int MinInterval = 1;
        public const int MaxInterval = 365;

        public const int MinQuality = 0;
        public const int MaxQuality = 5;
        public const int PassingQuality = 3;

        private const int FirstIntervalDays = 1;
        private const int SecondIntervalDays = 3;


        public static QualityResult ComputeRecallQuality(TimeSpan typeAverageTime, TimeSpan taskActionTime, int actionCounter, double similarity, double difficulty)
        {
            double penalty = similarity < 0.99d ? Math.Pow(similarity, 3.0d) : 1;
            double Accuracy = CalcAccuracySigmoid(similarity * penalty);

            double Stability = CalcStabilityExp(actionCounter);

            var timeRatio = CalcReactionRatio(taskActionTime, typeAverageTime);
            double Reaction = CalcReactionPow(timeRatio);

            double rawKnowledge = (0.20d * Accuracy + 0.45d * Stability + 0.35d * Reaction) * difficulty;
            double Quality = rawKnowledge * MaxQuality - 0.25d;

            if (Accuracy < 0.5d)
                Quality = MinQuality;
            else if (!IsPassingQuality(Quality) && Accuracy >= 0.8d)
                Quality = PassingQuality;

            Quality = Math.Clamp(Quality, 0, MaxQuality);
            Quality = Math.Round(Quality, 2);


            return new QualityResult()
            {
                Quality     = Quality,
                Difficulty  = difficulty,
                Accuracy    = Accuracy,
                Stability   = Stability,
                Reaction    = Reaction,
            };
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

        private static double CalcAccuracySigmoid(double similarity, double center = 0.7, double steepness = 12.0)
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

        private static double CalcReactionPow(double ratio, double min = 0.6, double max = 1.05, double steepness = 1.2)
        {
            double raw = Math.Pow(ratio, steepness);
            return Math.Clamp(raw, min, max);
        }
    }
}
