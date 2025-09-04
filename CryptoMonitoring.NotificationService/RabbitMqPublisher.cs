using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CryptoMonitoring.NotificationService
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly IConnection _conn;
        private readonly IModel _ch;
        private const string Exchange = "notifications";

        public RabbitMqPublisher(IRabbitMqConnectionFactory factory)
        {
            _conn = factory.CreateConnection();
            _ch = _conn.CreateModel();
            _ch.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
        }

        public void Publish(string eventType, object payload)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            var props = _ch.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;

            _ch.BasicPublish(exchange: Exchange, routingKey: eventType, basicProperties: props, body: body);
        }

        public void Dispose()
        {
            _ch?.Dispose();
            _conn?.Dispose();
        }
    }
}

