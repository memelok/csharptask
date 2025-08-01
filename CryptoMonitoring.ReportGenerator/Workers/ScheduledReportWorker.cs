using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Services;
using Microsoft.EntityFrameworkCore;


namespace CryptoMonitoring.ReportGenerator.Workers
{
    public class ScheduledReportWorker
    {
        private readonly IReportService _reportService;

        public ScheduledReportWorker(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task DailyJob()
        {
            var request = new ReportRequest
            {
                Type = ReportType.Daily,
                Date = DateTime.UtcNow.AddDays(-1)
            };
            var result = await _reportService.GenerateAsync(request);
            await SaveToDiskAsync(result);
        }

        public async Task WeeklyJob()
        {
            var request = new ReportRequest
            {
                Type = ReportType.Daily,
                Date = DateTime.UtcNow.AddDays(-7)
            };
            var result = await _reportService.GenerateAsync(request);
            await SaveToDiskAsync(result);
        }

        private static async Task SaveToDiskAsync(ReportResponse response)
        {
            var folder = Path.Combine("Reports", response.FileName.Substring(0, 8));
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, response.FileName);
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            await response.Stream.CopyToAsync(fs);
        }
    }
}