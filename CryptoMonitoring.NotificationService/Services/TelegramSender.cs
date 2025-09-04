using CryptoMonitoring.NotificationService.Infrastructure;
using CryptoMonitoring.NotificationService.Models;
using Microsoft.Extensions.Options;

namespace CryptoMonitoring.NotificationService.Services
{
    public class TelegramSender:IChannelSender
    {
        private readonly HttpClient _http;
        private readonly TelegramOptions _opts;

        public TelegramSender(HttpClient http, IOptions<TelegramOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
        }

        public NotificationChannel Channel => NotificationChannel.Telegram;

        public async Task SendAsync(string recipient, string subject, string body)
        {
            var url = $"{_opts.BaseUrl}/bot{_opts.BotToken}/sendMessage";
            var text = $"[{subject}]\n{body}";
            var payload = new { chat_id = recipient, text, parse_mode = "HTML", disable_web_page_preview = true };

            using var resp = await _http.PostAsJsonAsync(url, payload);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"telegram send failed: {(int)resp.StatusCode} {err}");
            }
        }
    }
}

