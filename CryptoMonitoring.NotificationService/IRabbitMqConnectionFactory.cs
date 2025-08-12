using RabbitMQ.Client;

namespace CryptoMonitoring.NotificationService
{
    public interface IRabbitMqConnectionFactory
    {
        IConnection CreateConnection();
    }
}
