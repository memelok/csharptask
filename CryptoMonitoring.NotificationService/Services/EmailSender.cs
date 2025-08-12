using CryptoMonitoring.NotificationService.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;



namespace CryptoMonitoring.NotificationService.Services
{
    public class EmailSender : IChannelSender
    {
        public NotificationChannel Channel => NotificationChannel.Email;
        private readonly EmailOptions _opts;

        public EmailSender(IOptions<EmailOptions> opts)
        {
            _opts = opts.Value;
        }

        public async Task SendAsync(string recipient, string subject, string body)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_opts.From));
            msg.To.Add(MailboxAddress.Parse(recipient));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_opts.Host, _opts.Port, _opts.UseSsl);
            if (!string.IsNullOrWhiteSpace(_opts.User))
                await client.AuthenticateAsync(_opts.User, _opts.Pass);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }

    public class EmailOptions
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string? User { get; set; }
        public string? Pass { get; set; }
        public string From { get; set; } = null!;
    }
}