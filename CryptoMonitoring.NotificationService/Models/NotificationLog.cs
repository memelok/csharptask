using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace CryptoMonitoring.NotificationService.Models
{
    public class NotificationLog
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public Guid SubscriptionId { get; set; }
        public string EventType { get; set; } = null!;
        public string Payload { get; set; } = null!;     
        public DateTime SentAt { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
