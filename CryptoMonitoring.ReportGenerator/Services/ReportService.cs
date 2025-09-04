using CryptoMonitoring.ReportGenerator.Builders;
using CryptoMonitoring.ReportGenerator.Data;      
using CryptoMonitoring.ReportGenerator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorLight;
using System.Text;

namespace CryptoMonitoring.ReportGenerator.Services
{
    public class ReportService : IReportService
    {
        private readonly IEnumerable<IReportBuilder> _builders;
        private readonly IRazorLightEngine _razor;
        private readonly ILogger<ReportService> _logger;
        private readonly ReportDbContext _db;      

        public ReportService(
            IEnumerable<IReportBuilder> builders,
            IRazorLightEngine razor,
            ILogger<ReportService> logger,
            ReportDbContext db)                  
        {
            _builders = builders;
            _razor = razor;
            _logger = logger;
            _db = db;
        }

        public async Task<ReportResponse> GenerateAsync(ReportRequest req)
        {
            try
            {
                _logger.LogInformation("Starting GenerateAsync with request: {@Request}", req);
                req.NormalizeDatesToUtc();

                var builder = _builders.FirstOrDefault(b => b.Type == req.Type)
                              ?? throw new InvalidOperationException($"Builder for {req.Type} not found");

                _logger.LogInformation("Using builder: {BuilderType}", builder.GetType().Name);

                var ms = await builder.BuildAsync(req);

                _logger.LogInformation("Report stream built successfully");

                return new ReportResponse
                {
                    Stream = ms,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = $"{req.Type}_{DateTime.UtcNow:yyyyMMddHHmm}.xlsx"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateAsync");
                throw;
            }
        }

        public async Task<string> GenerateDashboardAsync(ReportRequest req)
        {
            
            var items = await _db.Snapshots
                .Where(s => s.Timestamp >= req.StartDate && s.Timestamp <= req.EndDate)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html><html><head><meta charset='utf-8'><title>Dashboard</title></head><body>");
            sb.AppendLine($"<h1>Dashboard {req.StartDate:yyyy-MM-dd} – {req.EndDate:yyyy-MM-dd}</h1>");
            sb.AppendLine("<table border='1'><thead><tr><th>Timestamp</th><th>Symbol</th><th>Price</th></tr></thead><tbody>");

            foreach (var it in items)
            {
                sb.AppendLine("<tr>" +
                    $"<td>{it.Timestamp:yyyy-MM-dd HH:mm}</td>" +
                    $"<td>{it.Symbol}</td>" +
                    $"<td>{it.CurrentPrice:F2}</td>" +
                    "</tr>");
            }

            sb.AppendLine("</tbody></table></body></html>");
            return sb.ToString();
        }
    }
}
