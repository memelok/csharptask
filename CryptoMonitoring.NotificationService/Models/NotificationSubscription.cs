namespace CryptoMonitoring.NotificationService.Models
{
    public class NotificationSubscription
    {
        public Guid Id { get; set; }
        public string Recipient { get; set; } = null!;   
        public NotificationChannel Channel { get; set; }  
        public string EventType { get; set; } = null!;   
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum NotificationChannel
    {
        Email,
        Telegram,
        Push,
        Webhook
    }
}
