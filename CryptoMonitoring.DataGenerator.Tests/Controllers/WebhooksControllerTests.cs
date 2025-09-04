using System.Text;
using System.Text.Json;
using CryptoMonitoring.DataGenerator.Controllers;
using CryptoMonitoring.DataGenerator.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace DataGenerator.Tests.Controllers
{
    public class WebhooksControllerTests
    {
        [Fact]
        public void Test_ShouldPublishMessageAndReturnOk()
        {
            var channelMock = new Mock<IModel>();
            var propsMock = new Mock<IBasicProperties>();
            channelMock
                .Setup(ch => ch.CreateBasicProperties())
                .Returns(propsMock.Object);

            var controller = new WebhooksController(channelMock.Object);
            var payload = new CoinMarket
            {
                Id = "bitcoin",
                Symbol = "BTC",
                Name = "Bitcoin",
                CurrentPrice = 60000m,
                Change24h = 2.5
            };

            var result = controller.Test(payload) as OkObjectResult;

            result.StatusCode.Should().Be(200);
            result.Value.Should().BeEquivalentTo(new { success = true });

            propsMock.VerifySet(p => p.ContentType = "application/json", Times.Once);
            propsMock.VerifySet(p => p.DeliveryMode = 2, Times.Once);

            var expectedBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            channelMock.Verify(ch => ch.BasicPublish(
                "crypto-data",
                "market.prices",
                propsMock.Object,
                expectedBody
            ), Times.Once);
        }
    }
}