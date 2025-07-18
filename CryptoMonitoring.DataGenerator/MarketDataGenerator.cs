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

        public MarketDataGenerator(
            ICryptoApiClient api,
            IModel channel,
            IConfiguration cfg)
        {
            _api = api;
            _channel = channel;
            _interval = cfg.GetValue<int>("Generator:IntervalSeconds");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var markets = await _api.GetMarketsAsync(stoppingToken);

                var json = JsonSerializer.Serialize(markets);
                var body = Encoding.UTF8.GetBytes(json);

                var props = _channel.CreateBasicProperties();
                props.ContentType = "application/json";
                props.DeliveryMode = 2;  

                _channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: RoutingKey,
                    basicProperties: props,
                    body: body
                );

                await Task.Delay(TimeSpan.FromSeconds(_interval), stoppingToken);
            }
        }
    }
}
