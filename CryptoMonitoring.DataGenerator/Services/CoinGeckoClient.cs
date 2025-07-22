using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoMonitoring.DataGenerator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoMonitoring.DataGenerator.Services
{
    public class CoinGeckoClient : ICryptoApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<CoinGeckoClient> _log;
        private readonly string? _apiKey;

        public CoinGeckoClient(
            HttpClient httpClient,
            ILogger<CoinGeckoClient> log,
            IConfiguration cfg)
        {
            _http = httpClient;
            _log = log;
            _apiKey = cfg["Api:CoinGecko:ApiKey"];
        }
        public async Task<IEnumerable<CoinMarket>> GetMarketsAsync(CancellationToken ct)
        {
            var path = "coins/markets" +
                       "?vs_currency=usd" +
                       "&order=market_cap_desc" +
                       "&per_page=50" +
                       "&page=1" +
                       "&sparkline=false";

            var fullUrl = new Uri(_http.BaseAddress!, path);
            _log.LogInformation("🌐 Requesting: {Url}", fullUrl);

            //-----

            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            
            var response = await _http.SendAsync(request, ct);
            //-----
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError("❌ CoinGecko returned {StatusCode}: {Content}",
                              (int)response.StatusCode, content);
                throw new HttpRequestException($"API failure: {(int)response.StatusCode}");
            }

            _log.LogInformation("✔ CoinGecko responded 200");

            try
            {
                var markets = JsonSerializer.Deserialize<List<CoinMarket>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return markets ?? Enumerable.Empty<CoinMarket>();
            }
            catch (JsonException ex)
            {
                _log.LogError(ex, "❌ Failed to parse CoinGecko response");
                return Enumerable.Empty<CoinMarket>();
            }
        }

    }


}
