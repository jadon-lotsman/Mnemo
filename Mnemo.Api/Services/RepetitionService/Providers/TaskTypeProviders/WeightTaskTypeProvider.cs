using Microsoft.AspNetCore.Identity;
using Mnemo.Data.Entities;
using Mnemo.Shared.SM2Helper;

namespace Mnemo.Services.RepetitionService.Providers.TaskTypeProviders
{
    public class WeightTaskTypeProvider : ITaskTypeProvider
    {
        private readonly ILogger<WeightTaskTypeProvider> _logger;

        private static readonly (Type TaskType, double BaseWeigth, double Difficulty)[] TaskDifficulties = new[]
        {
            (typeof(YesOrNoRepetitionTask),         0.6d,   0.1d),
            (typeof(OptionRepetitionTask),          0.95d,  1.5d),
            (typeof(SyllableReorderRepetitionTask), 1.0d,   2.2d),
            (typeof(TextRepetitionTask),            1.0d,   2.5d),
            (typeof(SentenceReorderRepetitionTask), 0.3d,   3.0d)
        };


        public WeightTaskTypeProvider(ILogger<WeightTaskTypeProvider> logger)
        {
            _logger = logger;
        }



        public (Type taskType, bool isForward) GetType(double easinessFactor)
        {
            double targetDifficulty = CalcTargetDifficult(easinessFactor, 0.0d, TaskDifficulties.Last().Difficulty);

            double[] weights = TaskDifficulties
                .Select(t => t.BaseWeigth * CalcGaussianSimilarity(targetDifficulty, t.Difficulty, 0.6d))
                .ToArray();


            double sum = weights.Sum();
            double[] probabilities = weights.Select(w => w / sum).ToArray();

            double rand = Random.Shared.NextDouble();
            double cumulative = 0.0;
            Type selectedType = TaskDifficulties.Last().TaskType;

            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulative += probabilities[i];
                if (rand < cumulative)
                {
                    selectedType = TaskDifficulties[i].TaskType;
                    break;
                }
            }

            bool isForward = CalcIsForward(easinessFactor, 0.4d, 0.8d);


            _logger.LogDebug(
                "Provide with EasinessFactor:{EasinessFactor:F1}: TargetDifficult:{TargetDifficult:F1} (MaxDifficult:{MaxDifficulty:F1}) " +
                "resulting SelectedType:{SelectedType, -30} [{Probabilities}] ",
                easinessFactor,
                targetDifficulty,
                TaskDifficulties.Last().Difficulty,
                selectedType.Name,
                string.Join("|", probabilities.Select(p => p.ToString("F2")))
            );

            return (selectedType, isForward);
        }

        public double CalcGaussianSimilarity(double targetDifficulty, double taskDifficulty, double sigma)
        {
            return Math.Exp(-Math.Pow(taskDifficulty - targetDifficulty, 2) / (2 * sigma * sigma));
        }

        public double CalcTargetDifficult(double easinessFactor, double min, double max)
        {
            double normalized = (easinessFactor - SM2Helper.MinEF) / (SM2Helper.MaxEF - SM2Helper.MinEF);
            normalized = Math.Clamp(normalized, 0.0d, 1.0d);

            return (max - min) * normalized;
        }

        public bool CalcIsForward(double easinessFactor, double min, double max)
        {
            double normalized = (easinessFactor - SM2Helper.MinEF) / (SM2Helper.MaxEF - SM2Helper.MinEF);
            normalized = Math.Clamp(normalized, 0.0d, 1.0d);

            return Random.Shared.NextDouble() <= max - (max - min) * normalized;
        }
    }
}
