using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoMonitoring.DataGenerator;
using CryptoMonitoring.DataGenerator.Models;
using CryptoMonitoring.DataGenerator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace DataGenerator.Tests.Services
{
    public class TestableMarketDataGenerator : MarketDataGenerator
    {
        public TestableMarketDataGenerator(
            ICryptoApiClient api,
            IModel channel,
            IConfiguration config,
            ILogger<MarketDataGenerator> logger)
            : base(api, channel, config, logger) { }

        public Task InvokeExecuteAsync(CancellationToken token) => ExecuteAsync(token);
    }

    public class MarketDataGeneratorTests
    {
        [Fact]
        public async Task ExecuteAsync_PublishesOneMessagePerMarket_ThenStopsOnCancellation()
        {
            var cts = new CancellationTokenSource();
            int publishCount = 0;

            var apiMock = new Mock<ICryptoApiClient>();
            var markets = new List<CoinMarket>
            {
                new() { Id = "1", Symbol = "AAA", Name = "Aaa", CurrentPrice = 1m, Change24h = 0.1 },
                new() { Id = "2", Symbol = "BBB", Name = "Bbb", CurrentPrice = 2m, Change24h = 0.2 }
            };
            apiMock.Setup(a => a.GetMarketsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(markets);

            var channelMock = new Mock<IModel>();
            var propsMock = new Mock<IBasicProperties>();
            channelMock.Setup(ch => ch.CreateBasicProperties()).Returns(propsMock.Object);

            channelMock
                .Setup(ch => ch.BasicPublish(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IBasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>()))
                .Callback(() =>
                {
                    publishCount++;
                    if (publishCount >= markets.Count)
                        cts.Cancel();
                });

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c.GetValue<int>("Generator:IntervalSeconds")).Returns(1);

            var logger = new LoggerFactory().CreateLogger<MarketDataGenerator>();
            var generator = new TestableMarketDataGenerator(
                apiMock.Object, channelMock.Object, configMock.Object, logger);

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => generator.InvokeExecuteAsync(cts.Token));

            channelMock.Verify(ch => ch.BasicPublish(
                "crypto-data",
                "market.prices",
                false,
                propsMock.Object,
                It.Is<ReadOnlyMemory<byte>>(b => HasValidSymbol(b))
            ), Times.Exactly(markets.Count));

            apiMock.Verify(a => a.GetMarketsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private static bool HasValidSymbol(ReadOnlyMemory<byte> body)
        {
            var json = Encoding.UTF8.GetString(body.ToArray());
            var symbol = JsonSerializer.Deserialize<CoinMarket>(json)!.Symbol;
            return symbol == "AAA" || symbol == "BBB";
        }
    }
}
