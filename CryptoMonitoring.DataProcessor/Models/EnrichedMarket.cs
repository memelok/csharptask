using System;
using System.Text.Json.Serialization;
namespace CryptoMonitoring.DataProcessor.Models
{
    public class EnrichedMarket : CoinMarket 
    {
        public decimal? Sma7 { get; set; }
        public decimal? Sma21 { get; set; }

        public decimal? SupportLevel { get; set; }
        public decimal? ResistanceLevel { get; set; }

        public double? Volatility { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
