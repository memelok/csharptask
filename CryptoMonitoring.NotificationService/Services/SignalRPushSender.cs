using CryptoMonitoring.NotificationService.Hubs;
using CryptoMonitoring.NotificationService.Models;
using Microsoft.AspNetCore.SignalR;

namespace CryptoMonitoring.NotificationService.Services
{
    public class SignalRPushSender: IChannelSender
    {
        private readonly IHubContext<NotificationsHub> _hub;

        public SignalRPushSender(IHubContext<NotificationsHub> hub) => _hub = hub;

        public NotificationChannel Channel => NotificationChannel.Push;

        public Task SendAsync(string recipient, string subject, string body)
            => _hub.Clients.User(recipient).SendAsync("ReceiveNotification", new { subject, body });
    }
}
