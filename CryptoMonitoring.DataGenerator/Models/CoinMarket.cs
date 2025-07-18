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

        [JsonPropertyName("price_change_percentage_24h")]
        public double Change24h { get; set; }
    }
}
