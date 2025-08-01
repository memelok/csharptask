using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Utils;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using Microsoft.EntityFrameworkCore;



namespace CryptoMonitoring.ReportGenerator.Builders
{
    public class TechnicalReportBuilder : IReportBuilder
    {
        public ReportType Type => ReportType.Technical;

        private readonly ReportDbContext _db;

        public TechnicalReportBuilder(ReportDbContext db) => _db = db;

        public async Task<MemoryStream> BuildAsync(ReportRequest request)
        {
            var start = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var end = request.EndDate ?? DateTime.UtcNow;
            var symbols = request.Symbols?.Distinct().ToArray()
                          ?? new[] { "btc", "eth" };

            var data = await _db.Snapshots
                .Where(s => symbols.Contains(s.Symbol) && s.Timestamp >= start && s.Timestamp <= end)
                .OrderBy(s => s.Timestamp)
                .ToListAsync();

            var ms = new MemoryStream();
            ExcelPackage.License.SetNonCommercialPersonal("CryptoMonitoring");
            using var pkg = new ExcelPackage();
            var ws = pkg.Workbook.Worksheets.Add("Technical Analysis");

            ws.Cells["A1"].Value = "Timestamp";
            ws.Cells["B1"].Value = "Symbol";
            ws.Cells["C1"].Value = "ClosePrice";
            ws.Cells["D1"].Value = "SMA(10)";

            int row = 2;
            foreach (var grp in data.GroupBy(x => x.Symbol))
            {
                var prices = grp.Select(x => x.CurrentPrice).ToList();
                var sma = ExcelHelper.ComputeSMA(prices, period: 10);

                for (int i = 0; i < grp.Count(); i++)
                {
                    var item = grp.ElementAt(i);
                    ws.Cells[row, 1].Value = item.Timestamp;
                    ws.Cells[row, 2].Value = item.Symbol;
                    ws.Cells[row, 3].Value = (double)item.CurrentPrice;
                    ws.Cells[row, 4].Value = (i < sma.Count ? (double)sma[i] : (double?)null);
                    row++;
                }
            }

            var chart = ws.Drawings.AddChart("smaChart", eChartType.Line);
            chart.SetPosition(1, 0, 5, 0);
            chart.Title.Text = "SMA(10) by Symbol";
            chart.Series.Add($"D2:D{row - 1}", $"A2:A{row - 1}");

            pkg.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
    
}
}
