using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MongoDB.Driver;


using CryptoMonitoring.DataProcessor.Models;
using CryptoMonitoring.DataProcessor.Services;
using System.Threading.Channels;


namespace CryptoMonitoring.DataProcessor
{
    public class MarketMessageConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly ILogger<MarketMessageConsumer> _log;
        private readonly IServiceProvider _services;

        public MarketMessageConsumer(IModel channel, IServiceProvider services, ILogger<MarketMessageConsumer> log)
        {
            _channel = channel;
            _services = services;
            _log = log;
        }

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            _channel.BasicQos(0, 10, false); 

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var data = JsonSerializer.Deserialize<CoinMarket>(json);
                    var raw = JsonSerializer.Deserialize<CoinMarket>(json);
                    _log.LogInformation("📥 Received {Symbol}", raw?.Symbol);

                    using var scope = _services.CreateScope();
                    var validator = scope.ServiceProvider.GetRequiredService<MarketDataValidator>();
                    if (!validator.IsValid(raw))
                    {
                        _log.LogWarning("⚠ Invalid data for {Symbol}, ack and skip", raw.Symbol);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    var enricher = scope.ServiceProvider.GetRequiredService<MarketDataEnricher>();
                    var enriched = enricher.Enrich(raw);

                    var storage = scope.ServiceProvider.GetRequiredService<MarketDataStorage>();
                    await storage.SaveAsync(enriched);
                    _log.LogInformation("✔ Saved & persisted {Symbol}", enriched.Symbol);

                    var cache = scope.ServiceProvider.GetRequiredService<RedisCacheService>();
                    cache.Set(enriched.Symbol, enriched);
                    _log.LogInformation("✔ Cached {Symbol}", enriched.Symbol);

                    var anomaly = scope.ServiceProvider.GetRequiredService<AnomalyDetector>();
                    if (anomaly.IsAnomalous(enriched))
                        _log.LogWarning("🚨 Anomaly detected for {Symbol}", enriched.Symbol);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "❌ Error processing message, requeueing");
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }

            };

            _channel.BasicConsume(queue: "market.prices", autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
        public override void Dispose()
        {
            _channel.Close();
            base.Dispose();
        }
    }
}
