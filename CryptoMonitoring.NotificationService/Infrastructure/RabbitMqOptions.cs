namespace CryptoMonitoring.NotificationService.Infrastructure
{
    public class RabbitMqOptions
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
    }
}
