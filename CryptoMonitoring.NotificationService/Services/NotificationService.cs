using CryptoMonitoring.NotificationService.Data;
using CryptoMonitoring.NotificationService.Models;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CryptoMonitoring.NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationsDbContext _pg;
        private readonly IMongoCollection<NotificationLog> _logCollection;
        private readonly IEnumerable<IChannelSender> _senders;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            NotificationsDbContext pgContext,
            IMongoClient mongoClient,
            IEnumerable<IChannelSender> senders,
            ILogger<NotificationService> logger)
        {
            _pg = pgContext;
            _logCollection = mongoClient.GetDatabase("notifications").GetCollection<NotificationLog>("logs");
            _senders = senders;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(NotificationEnvelope envelope) where TEvent : class
        {
            var subs = await _pg.Subscriptions
                .Where(s => s.EventType == envelope.EventType && s.IsActive)
                .ToListAsync();

            if (subs.Count == 0)
            {
                _logger.LogInformation("No active subscriptions for EventType={EventType}", envelope.EventType);
                return;
            }

            foreach (var sub in subs)
            {
                var subject = envelope.EventType;
                var body = envelope.Payload;

                var log = new NotificationLog
                {
                    SubscriptionId = sub.Id,
                    EventType = envelope.EventType,
                    Payload = envelope.Payload,
                    SentAt = DateTime.UtcNow
                };

                try
                {
                    var sender = _senders.First(x => x.Channel == sub.Channel);
                    _logger.LogInformation("Sending {EventType} via {Channel} to {Recipient}", envelope.EventType, sub.Channel, sub.Recipient);
                    await sender.SendAsync(sub.Recipient, subject, body);
                    log.Success = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send {EventType} via {Channel} to {Recipient}", envelope.EventType, sub.Channel, sub.Recipient);
                    log.Success = false;
                    log.ErrorMessage = ex.Message;
                }

                await _logCollection.InsertOneAsync(log);
            }
        }

        public async Task AlertAsync(AlertTriggeredEvent alert, CancellationToken ct = default)
        {
            var payload = JsonSerializer.Serialize(alert);

            var envelope = new NotificationEnvelope(
                eventType: nameof(AlertTriggeredEvent),
                payload: payload
            );

            await PublishAsync<AlertTriggeredEvent>(envelope);
        }
    }
}
