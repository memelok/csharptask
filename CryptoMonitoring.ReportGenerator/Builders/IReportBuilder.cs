using CryptoMonitoring.ReportGenerator.Models;

namespace CryptoMonitoring.ReportGenerator.Builders
{
    public interface IReportBuilder
    {
        ReportType Type { get; }
        Task<MemoryStream> BuildAsync(ReportRequest request);
    }
}
