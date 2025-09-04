namespace CryptoMonitoring.NotificationService
{
    public interface IRabbitMqPublisher
    {
        void Publish(string eventType, object payload);
    }
}
