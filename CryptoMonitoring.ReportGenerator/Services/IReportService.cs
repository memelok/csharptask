using CryptoMonitoring.ReportGenerator.Models;

namespace CryptoMonitoring.ReportGenerator.Services
{
    public interface IReportService
    {
        Task<ReportResponse> GenerateAsync(ReportRequest request);
        Task<string> GenerateDashboardAsync(ReportRequest request);
    }
}
