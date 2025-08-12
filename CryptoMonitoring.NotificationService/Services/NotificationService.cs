using CryptoMonitoring.NotificationService.Data;
using CryptoMonitoring.NotificationService.Models;
using System.Text.Json;
using MongoDB.Driver;
using RazorLight;
using Microsoft.EntityFrameworkCore;


namespace CryptoMonitoring.NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationsDbContext _pg;
        private readonly IMongoCollection<NotificationLog> _logCollection;
        private readonly IRazorLightEngine _razor;
        private readonly IEnumerable<IChannelSender> _senders;

        public NotificationService(
            NotificationsDbContext pgContext,
            IMongoClient mongoClient,
            IRazorLightEngine razor,
            IEnumerable<IChannelSender> senders)
        {
            _pg = pgContext;
            _logCollection = mongoClient
                .GetDatabase("notifications")
                .GetCollection<NotificationLog>("logs");
            _razor = razor;
            _senders = senders;
        }

        public async Task PublishAsync<TEvent>(NotificationEnvelope envelope) where TEvent : class
        {
            var subs = await _pg.Subscriptions
                .Where(s => s.EventType == envelope.EventType && s.IsActive)
                .ToListAsync();

            foreach (var sub in subs)
            {
                var templateKey = $"{envelope.EventType}_{sub.Channel}.cshtml";
                var model = JsonSerializer.Deserialize<TEvent>(envelope.Payload)!;
                var body = await _razor.CompileRenderAsync(templateKey, model);
                var subject = envelope.EventType;

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
                    await sender.SendAsync(sub.Recipient, subject, body);
                    log.Success = true;
                }
                catch (Exception ex)
                {
                    log.Success = false;
                    log.ErrorMessage = ex.Message;
                }

                await _logCollection.InsertOneAsync(log);
            }
        }
    }
}