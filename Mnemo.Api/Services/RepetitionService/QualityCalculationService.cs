using Microsoft.Extensions.Options;

namespace Mnemo.Services.RepetitionService
{
    public class QualityCalculationService
    {
        private readonly IOptions<SM2Options> _sm2;

        public QualityCalculationService(IOptions<SM2Options> sm2)
        {
            _sm2 = sm2;
        }



        public QualityResult ComputeTaskQuality(TimeSpan typeAverageTime, TimeSpan taskActionTime, int actionCounter, double similarity, double difficulty)
        {
            double penalty = similarity < 0.99d ? Math.Pow(similarity, 3.0d) : 1;
            double Accuracy = CalcAccuracySigmoid(similarity * penalty);

            double Stability = CalcStabilityExp(actionCounter);

            var timeRatio = CalcReactionRatio(taskActionTime, typeAverageTime);
            double Reaction = CalcReactionPow(timeRatio);

            double rawKnowledge = (0.20d * Accuracy + 0.45d * Stability + 0.35d * Reaction) * difficulty;
            double Quality = rawKnowledge * _sm2.Value.MaxQuality - 0.25d;

            if (Accuracy < 0.5d)
                Quality = _sm2.Value.MinQuality;
            else if (!_sm2.Value.IsPassingQuality(Quality) && Accuracy >= 0.8d)
                Quality = _sm2.Value.PassingQuality;

            Quality = Math.Clamp(Quality, 0, _sm2.Value.MaxQuality);
            Quality = Math.Round(Quality, 2);


            return new QualityResult()
            {
                Quality = Quality,
                Difficulty = difficulty,
                Accuracy = Accuracy,
                Stability = Stability,
                Reaction = Reaction,
            };
        }


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
