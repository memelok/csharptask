using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using CryptoMonitoring.NotificationService.Models;



namespace CryptoMonitoring.NotificationService.Services
{
    public class NotificationWorker : BackgroundService
    {
        private readonly IRabbitMqConnectionFactory _factory;
        private readonly INotificationService _notificationService;
        private IModel? _channel;

        public NotificationWorker(
            IRabbitMqConnectionFactory factory,
            INotificationService notificationService)
        {
            _factory = factory;
            _notificationService = notificationService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = _factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.ExchangeDeclare("notifications", ExchangeType.Topic, durable: true);
            _channel.QueueDeclare("notification_queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("notification_queue", "notifications", "#");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var payload = Encoding.UTF8.GetString(body);
                var envelope = new NotificationEnvelope(ea.RoutingKey, payload);

                await _notificationService.PublishAsync<NotificationEnvelope>(envelope);


                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume("notification_queue", autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
    }
}
    