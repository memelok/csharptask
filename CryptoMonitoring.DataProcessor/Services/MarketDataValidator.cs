using CryptoMonitoring.DataProcessor.Models;


namespace CryptoMonitoring.DataProcessor.Services
{
    public class MarketDataValidator
    {
        public bool IsValid(CoinMarket market) => market != null && !string.IsNullOrWhiteSpace(market.Symbol);
    }
}
