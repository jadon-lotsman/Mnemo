namespace Mnemo.Services.EnrichmentService
{
    public class EnrichmentOptions
    {
        public bool IsEnabled { get; set; } = true;
        public int BatchSize { get; set; } = 5;
        public int BatchDelaySeconds { get; set; } = 150;
        public int RequestDelayMilliseconds { get; set; } = 800;
        public int StuckProcessingTimeoutMinutes { get; set; } = 10;
    }
}
