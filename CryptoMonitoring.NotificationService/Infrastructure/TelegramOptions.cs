namespace CryptoMonitoring.NotificationService.Infrastructure
{
    public class TelegramOptions
    {
        public string BotToken { get; set; } = null!;
        public string? BaseUrl { get; set; } = "https://api.telegram.org";
    }
}
