using CryptoMonitoring.DataGenerator.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace CryptoMonitoring.DataGenerator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IModel _channel;

        public WebhooksController(IModel channel)
        {
            _channel = channel;
        }

        [HttpPost("test")]
        public IActionResult Test([FromBody] CoinMarket payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(json);
            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;

            _channel.BasicPublish(
                exchange: "crypto-data",
                routingKey: "market.prices",
                basicProperties: props,
                body: body
            );

            return Ok(new { success = true });
        }
    }
}
