using System.Threading.Tasks;
using CryptoMonitoring.DataProcessor.Models;
using MongoDB.Driver;

namespace CryptoMonitoring.DataProcessor.Services
{
    public class MarketDataStorage 
    {
        private readonly IMongoCollection<EnrichedMarket> _mongo;
        private readonly PostgresDbContext _pg;

        public MarketDataStorage(
            IConfiguration cfg,
            PostgresDbContext pg)
        {
            _pg = pg;
            var conn = cfg.GetConnectionString("MongoDb");
            var client = new MongoClient(conn);
            _mongo = client.GetDatabase("crypto_db")
                           .GetCollection<EnrichedMarket>("snapshots");
        }

        public async Task SaveAsync(EnrichedMarket m)
        {
            await _mongo.InsertOneAsync(m);
            _pg.Snapshots.Add(m);
            await _pg.SaveChangesAsync();
        }
    }
}
