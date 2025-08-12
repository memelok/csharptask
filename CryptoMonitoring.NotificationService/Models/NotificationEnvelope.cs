namespace CryptoMonitoring.NotificationService.Models
{
    public class NotificationEnvelope
    {
        public string EventType { get; }
        public string Payload { get; }

        public NotificationEnvelope(string eventType, string payload)
        {
            EventType = eventType;
            Payload = payload;
        }
    }
}
