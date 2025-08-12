using CryptoMonitoring.NotificationService.Models;

namespace CryptoMonitoring.NotificationService
{
    public interface INotificationService
    {
        Task PublishAsync<TEvent>(NotificationEnvelope envelope) where TEvent : class;
    }
}
