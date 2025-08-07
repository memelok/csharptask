using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Builders;
using RazorLight;
using Microsoft.Extensions.Logging;

namespace CryptoMonitoring.ReportGenerator.Services
{
    public class ReportService : IReportService
    {
        private readonly IEnumerable<IReportBuilder> _builders;
        private readonly IRazorLightEngine _razor;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IEnumerable<IReportBuilder> builders,
            IRazorLightEngine razor,
            ILogger<ReportService> logger)
        {
            _builders = builders;
            _razor = razor;
            _logger = logger;
        }

        public async Task<ReportResponse> GenerateAsync(ReportRequest req)
        {
            req.NormalizeDatesToUtc();
            var builder = _builders
                .FirstOrDefault(b => b.Type == req.Type)
                ?? throw new InvalidOperationException("Builder not found");

            var ms = await builder.BuildAsync(req);
            return new ReportResponse
            {
                Stream = ms,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"{req.Type}_{DateTime.UtcNow:yyyyMMddHHmm}.xlsx"
            };
        }

        public async Task<string> GenerateDashboardAsync(ReportRequest req)
        {
            try
            {
                string templateKey = "Dashboard.cshtml";
                var model = new { Start = req.StartDate, End = req.EndDate };

                _logger.LogInformation("Rendering template {Key} with model {Start} - {End}", templateKey, model.Start, model.End);

                var html = await _razor.CompileRenderAsync(templateKey, model);

                _logger.LogInformation("Template rendered successfully");
                return html;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard generation failed");
                throw;
            }
        }
    }
}
