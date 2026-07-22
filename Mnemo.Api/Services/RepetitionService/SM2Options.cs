namespace Mnemo.Services.RepetitionService
{
    public class SM2Options
    {
        public double MinEF { get; set; } = 1.3d;
        public double MaxEF { get; set; } = 3.2d;
        public double InitEF { get; set; } = 2.5d;

        public int MinInterval { get; set; } = 1;
        public int MaxInterval { get; set; } = 365;
        public int FirstIntervalDays { get; set; } = 1;
        public int SecondIntervalDays { get; set; } = 3;

        public int MinQuality { get; set; } = 0;
        public int MaxQuality { get; set; } = 5;
        public int PassingQuality { get; set; } = 3;
        public bool IsPassingQuality(double quality) => quality >= PassingQuality;
    }
}
