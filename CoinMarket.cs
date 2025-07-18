using System.Text.Json.Serialization;

namespace CryptoMonitoring.DataGenerator.Models
{
    public class CoinMarket
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; set; }

        [JsonPropertyName("market_cap")]
        public decimal MarketCap { get; set; }

        [JsonPropertyName("price_change_percentage_24h")]
        public double PriceChangePercentage24h { get; set; }

        // другие пол€ из API:
        // [JsonPropertyName("total_volume")] public decimal TotalVolume { get; set; }
        // [JsonPropertyName("high_24h")]     public decimal High24h     { get; set; }
        // Е
    }
}
