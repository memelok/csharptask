using CryptoMonitoring.NotificationService.Models;

namespace CryptoMonitoring.NotificationService
{
    public interface INotificationService
    {
        Task PublishAsync<TEvent>(NotificationEnvelope envelope) where TEvent : class;

        // новый метод для триггера алертов
        Task AlertAsync(AlertTriggeredEvent alert, CancellationToken ct = default);
    }
}