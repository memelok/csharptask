using CryptoMonitoring.NotificationService.Models;

namespace CryptoMonitoring.NotificationService
{
    public interface IChannelSender
    {
        NotificationChannel Channel { get; }
        Task SendAsync(string recipient, string subject, string body);
    }
}
