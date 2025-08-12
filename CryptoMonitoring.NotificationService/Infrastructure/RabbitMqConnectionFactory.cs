using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace CryptoMonitoring.NotificationService.Infrastructure
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly RabbitMqOptions _opts;
        private readonly ConnectionFactory _factory;

        public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> opts)
        {
            _opts = opts.Value;
            _factory = new ConnectionFactory
            {
                HostName = _opts.Host,
                Port = _opts.Port,
                UserName = _opts.User,
                Password = _opts.Password
            };
        }

        public IConnection CreateConnection()
        {
            const int maxAttempts = 5;
            var delay = TimeSpan.FromSeconds(5);

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    return _factory.CreateConnection();
                }
                catch (BrokerUnreachableException) when (attempt < maxAttempts)
                {
                    Console.WriteLine($"[WARN] RabbitMQ недоступен, попытка {attempt}/{maxAttempts}. Жду {delay.TotalSeconds} сек…");
                    Thread.Sleep(delay);
                }
            }
        }



    }
}
