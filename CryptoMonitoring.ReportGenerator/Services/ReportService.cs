using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Builders;
using RazorLight;

namespace CryptoMonitoring.ReportGenerator.Services
{
    public class ReportService : IReportService
    {
        private readonly IEnumerable<IReportBuilder> _builders;
        private readonly IRazorLightEngine _razor;

        public ReportService(
            IEnumerable<IReportBuilder> builders,
            IRazorLightEngine razor)
        {
            _builders = builders;
            _razor = razor;
        }

        public async Task<ReportResponse> GenerateAsync(ReportRequest req)
        {
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
            string templateKey = $"{req.Type}Dashboard.cshtml";
            var model = new { Start = req.StartDate, End = req.EndDate };
            return await _razor.CompileRenderAsync(templateKey, model);
        }
    }
}
