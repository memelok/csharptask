using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoMonitoring.DataGenerator.Models;
using Serilog;

namespace CryptoMonitoring.DataGenerator.Services
{
    public class CoinGeckoClient : ICryptoApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<CoinGeckoClient> _log;
        private readonly string _apiKey;

        public CoinGeckoClient(HttpClient httpClient, ILogger<CoinGeckoClient> log, IConfiguration cfg)
        {
            _http = httpClient;
            _log = log;
            _apiKey = cfg["Api:CoinGecko:ApiKey"];
        }
        public async Task<IEnumerable<CoinMarket>> GetMarketsAsync(CancellationToken ct)
        {
            var url = $"/coins/markets?vs_currency=usd&x_cg_demo_api_key={_apiKey}";
            var response = await _http.GetAsync(url, ct);

            var content = await response.Content.ReadAsStringAsync(ct);

            _log.LogInformation(
                "API GET {Url} responded {StatusCode}: {Content}",
                response.RequestMessage?.RequestUri,
                (int)response.StatusCode,
                content);

            response.EnsureSuccessStatusCode();

            var markets = JsonSerializer.Deserialize<List<CoinMarket>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return markets ?? Enumerable.Empty<CoinMarket>();
        }
    }
}
