using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CryptoMonitoring.NotificationService.Models
{
    public enum ThresholdOperator
    {
        GreaterOrEqual = 0,
        LessOrEqual = 1
    }

    public class AlertChannels
    {
        public bool Email { get; set; }
        public bool Telegram { get; set; }
        public bool Webhook { get; set; }
        public bool SignalR { get; set; }
    }

    public class AlertTargets
    {
        public string? EmailTo { get; set; }
        public string? TelegramChatId { get; set; }
        public string? WebhookUrl { get; set; }
        public string? SignalRUserId { get; set; }
    }

    public class AlertRule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;

        [BsonElement("metric")]
        public string Metric { get; set; } = default!; 

        [BsonElement("operator")]
        public ThresholdOperator Operator { get; set; }

        [BsonElement("threshold")]
        public double Threshold { get; set; }

        [BsonElement("enabled")]
        public bool Enabled { get; set; } = true;

        [BsonElement("cooldownSeconds")]
        public int CooldownSeconds { get; set; } = 60;

        [BsonElement("channels")]
        public AlertChannels Channels { get; set; } = new();

        [BsonElement("targets")]
        public AlertTargets Targets { get; set; } = new();

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("lastTriggeredAt")]
        public DateTime? LastTriggeredAt { get; set; }

        [BsonElement("severity")]
        public string Severity { get; set; } = "info"; 
    }
}
