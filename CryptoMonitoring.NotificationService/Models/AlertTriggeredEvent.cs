namespace CryptoMonitoring.NotificationService.Models
{
    public class AlertTriggeredEvent
    {
        public string RuleId { get; set; } = default!;
        public string Metric { get; set; } = default!;
        public ThresholdOperator Operator { get; set; }
        public double Threshold { get; set; }
        public double CurrentValue { get; set; }
        public string Severity { get; set; } = "info";
        public string? Description { get; set; }
        public AlertChannels Channels { get; set; } = new();
        public AlertTargets Targets { get; set; } = new();

        public DateTime OccurredAtUtc { get; set; }
    }
}
