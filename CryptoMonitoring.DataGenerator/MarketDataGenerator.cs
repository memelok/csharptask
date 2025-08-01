using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoMonitoring.DataGenerator.Models;
using CryptoMonitoring.DataGenerator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace CryptoMonitoring.DataGenerator
{
    public class MarketDataGenerator : BackgroundService
    {
        private const string ExchangeName = "crypto-data";
        private const string RoutingKey = "market.prices";

        private readonly ICryptoApiClient _api;
        private readonly IModel _channel;
        private readonly int _interval;
        private readonly ILogger<MarketDataGenerator> _log;

        public MarketDataGenerator(
            ICryptoApiClient api,
            IModel channel,
            IConfiguration cfg,
            ILogger<MarketDataGenerator> log)
        {
            _api = api;
            _channel = channel;
            _interval = cfg.GetValue<int>("Generator:IntervalSeconds");
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation("🟢 MarketDataGenerator started");

            while (!stoppingToken.IsCancellationRequested)
            {


                try
                {
                    var markets = await _api.GetMarketsAsync(stoppingToken);
                    foreach (var market in markets)
                    {
                        var json = JsonSerializer.Serialize(market); // ✔️ правильно: один объект
                        var body = Encoding.UTF8.GetBytes(json);

                        var props = _channel.CreateBasicProperties();
                        props.ContentType = "application/json";
                        props.DeliveryMode = 2;

                        _channel.BasicPublish(
                            exchange: ExchangeName,
                            routingKey: RoutingKey,
                            basicProperties: props,
                            body: body);

                        _log.LogInformation("📤 Published market: {Symbol}", market.Symbol);
                    }
                    _log.LogInformation("✔ Published {Count} entries", markets.Count());
                }
                catch (Exception ex) 
                {
                    _log.LogError(ex, "❌ Failed to publish market data");
                }



                await Task.Delay(TimeSpan.FromSeconds(_interval), stoppingToken);
            }
        }
    }
}
