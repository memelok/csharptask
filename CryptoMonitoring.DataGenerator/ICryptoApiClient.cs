using CryptoMonitoring.DataGenerator.Models;

namespace CryptoMonitoring.DataGenerator
{
    public interface ICryptoApiClient
    {
        Task<IEnumerable<CoinMarket>> GetMarketsAsync(CancellationToken ct);
    }
}
