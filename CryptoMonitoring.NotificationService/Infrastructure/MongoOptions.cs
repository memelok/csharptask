namespace CryptoMonitoring.NotificationService.Infrastructure
{
    public class MongoOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string Database { get; set; } = null!;
    }
}
