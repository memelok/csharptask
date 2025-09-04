using CryptoMonitoring.NotificationService.Models;

namespace CryptoMonitoring.NotificationService.Infrastructure
{
    public interface IAlertRuleRepository
    {
        Task<IReadOnlyList<AlertRule>> GetAllEnabledAsync(CancellationToken ct = default);

        Task UpdateLastTriggeredAsync(string id, DateTime triggeredAt, CancellationToken ct = default);
    }
}

