using System.Threading.Tasks;
using CryptoMonitoring.DataProcessor.Models;
using MongoDB.Driver;

namespace CryptoMonitoring.DataProcessor.Services
{
    public class MarketDataStorage
    {
        //private readonly IMongoCollection<EnrichedMarket> _mongo;
        //private readonly PostgresDbContext _pg;

        //public MarketDataStorage(
        //    IConfiguration cfg,
        //    PostgresDbContext pg)
        //{
        //    _pg = pg;
        //    var conn = cfg.GetConnectionString("MongoDb");
        //    var client = new MongoClient(conn);
        //    _mongo = client.GetDatabase("crypto_db")
        //                   .GetCollection<EnrichedMarket>("snapshots");
        //}

        //public async Task SaveAsync(EnrichedMarket m)
        //{
        //    await _mongo.InsertOneAsync(m);
        //    _pg.Snapshots.Add(m);
        //    await _pg.SaveChangesAsync();
        //}
        
        private readonly IMongoCollection<EnrichedMarket> _collection;
        private readonly PostgresDbContext _pg;

        public MarketDataStorage(
            IMongoClient mongoClient,
            IConfiguration config,
            PostgresDbContext pg)
        {
            _pg = pg;
            var databaseName = config["Mongo:Database"]
                               ?? throw new ArgumentException("Mongo:Database not configured");
            _collection = mongoClient
                .GetDatabase(databaseName)
                .GetCollection<EnrichedMarket>("snapshots");
        }

        public async Task SaveAsync(EnrichedMarket m)
        {
            var filter = Builders<EnrichedMarket>.Filter.Eq(x => x.Id, m.Id);
            var options = new ReplaceOptions { IsUpsert = true };
            await _collection.ReplaceOneAsync(filter, m, options);

            _pg.Snapshots.Add(m);
            await _pg.SaveChangesAsync();
        }

    }
}
