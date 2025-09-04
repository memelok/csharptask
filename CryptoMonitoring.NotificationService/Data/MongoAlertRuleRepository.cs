using CryptoMonitoring.NotificationService.Infrastructure;
using CryptoMonitoring.NotificationService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CryptoMonitoring.NotificationService.Data
{
    public class MongoAlertRuleRepository : IAlertRuleRepository
    {
        private readonly IMongoCollection<AlertRule> _collection;

        public MongoAlertRuleRepository(IMongoClient client, IOptions<MongoSettings> options)
        {
            var db = client.GetDatabase(options.Value.DatabaseName);
            _collection = db.GetCollection<AlertRule>(options.Value.AlertRulesCollectionName);
        }

        public async Task<IReadOnlyList<AlertRule>> GetAllEnabledAsync(CancellationToken ct = default)
        {
            var filter = Builders<AlertRule>.Filter.Eq(x => x.Enabled, true);
            var cursor = await _collection.FindAsync(filter, cancellationToken: ct);
            return await cursor.ToListAsync(ct);
        }

        public Task UpdateLastTriggeredAsync(string id, DateTime triggeredAt, CancellationToken ct = default)
        {
            var filter = Builders<AlertRule>.Filter.Eq(x => x.Id, id);
            var update = Builders<AlertRule>.Update.Set(x => x.LastTriggeredAt, triggeredAt);
            return _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        }
    }
}