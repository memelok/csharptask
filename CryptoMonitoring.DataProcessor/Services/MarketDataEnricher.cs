using CryptoMonitoring.DataProcessor.Models;
using MongoDB.Driver;


namespace CryptoMonitoring.DataProcessor.Services
{
    public class MarketDataEnricher
    {

        private readonly IMongoCollection<EnrichedMarket> _coll;

        public MarketDataEnricher(IConfiguration cfg)
        {
            var conn = cfg.GetConnectionString("MongoDb");
            var client = new MongoClient(conn);
            _coll = client.GetDatabase("crypto_db")
                          .GetCollection<EnrichedMarket>("snapshots");
        }

        public EnrichedMarket Enrich(CoinMarket m)
        {
            var enriched = new EnrichedMarket
            {
                Id = m.Id,
                Symbol = m.Symbol,
                Name = m.Name,
                CurrentPrice = m.CurrentPrice,
                Change24h = m.Change24h,
                Timestamp = DateTime.UtcNow
            };

            var history = _coll
                .Find(x => x.Symbol == m.Symbol)
                .SortByDescending(x => x.Timestamp)
                .Limit(21)
                .ToList();

            if (history.Count >= 7)
                enriched.Sma7 = history.Take(7).Average(x => x.CurrentPrice);

            if (history.Count == 21)
                enriched.Sma21 = history.Average(x => x.CurrentPrice);

            if (history.Any())
            {
                enriched.SupportLevel = history.Min(x => x.CurrentPrice);
                enriched.ResistanceLevel = history.Max(x => x.CurrentPrice);
            }

            if (history.Count > 1)
            {
                var avg = history.Average(x => (double)x.CurrentPrice);
                enriched.Volatility = Math.Sqrt(
                    history.Sum(x => Math.Pow((double)x.CurrentPrice - avg, 2))
                    / (history.Count - 1)
                );
            }

            return enriched;
        }
    }
}
