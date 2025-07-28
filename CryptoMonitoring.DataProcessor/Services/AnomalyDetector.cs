using CryptoMonitoring.DataProcessor.Models;


namespace CryptoMonitoring.DataProcessor.Services
{
    public class AnomalyDetector
    {
        public bool IsAnomalous(EnrichedMarket m) =>
            m.Change24h is > 15 or < -15 || m.Volatility > 0.05;
    }
}
