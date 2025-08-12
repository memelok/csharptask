using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Data;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using Microsoft.EntityFrameworkCore;


namespace CryptoMonitoring.ReportGenerator.Builders
{
    public class DailyReportBuilder : IReportBuilder
    {
        public ReportType Type => ReportType.Daily;

        private readonly ReportDbContext _db;

        public DailyReportBuilder(ReportDbContext db) => _db = db;

        public async Task<MemoryStream> BuildAsync(ReportRequest request)
        {
            if (request.Date == null)
                throw new ArgumentException("Date is required");

            var date = DateTime.SpecifyKind(request.Date.Value.Date, DateTimeKind.Utc);

            var data = await _db.Snapshots
                .Where(s => s.Timestamp.Date == date)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var pkg = new ExcelPackage();
            var ws = pkg.Workbook.Worksheets.Add("Daily");
            ws.Cells["A1"].LoadFromCollection(data, true);

            int count = data.Count;
            var chart = ws.Drawings.AddChart("PriceChart", eChartType.Line);
            chart.Series.Add($"C2:C{count + 1}", $"A2:A{count + 1}");
            chart.Title.Text = "Closing Price";

            var ms = new MemoryStream();
            pkg.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
