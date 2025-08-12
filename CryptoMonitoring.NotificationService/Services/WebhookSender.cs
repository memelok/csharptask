using CryptoMonitoring.NotificationService.Models;
using System.Text;
using System.Text.Json;

namespace CryptoMonitoring.NotificationService.Services
{
    public class WebhookSender : IChannelSender
    {
        public NotificationChannel Channel => NotificationChannel.Webhook;
        private readonly HttpClient _client;

        public WebhookSender(HttpClient client)
        {
            _client = client;
        }

        public async Task SendAsync(string recipient, string subject, string body)
        {
            var payload = new
            {
                subject,
                body
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync(recipient, content);
            res.EnsureSuccessStatusCode();
        }
    }
}
