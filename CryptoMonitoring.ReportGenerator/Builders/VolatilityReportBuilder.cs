using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Models;
using OfficeOpenXml;
using CryptoMonitoring.ReportGenerator.Utils;
using Microsoft.EntityFrameworkCore;


namespace CryptoMonitoring.ReportGenerator.Builders
{
    public class VolatilityReportBuilder : IReportBuilder
    {
        public ReportType Type => ReportType.Volatility;

        private readonly ReportDbContext _db;

        public VolatilityReportBuilder(ReportDbContext db) => _db = db;

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
            var ws = pkg.Workbook.Worksheets.Add("Volatility");

            ws.Cells["A1"].Value = "Symbol";
            ws.Cells["B1"].Value = "StdDev";

            int row = 2;
            foreach (var grp in data.GroupBy(s => s.Symbol))
            {
                var prices = grp.Select(x => x.CurrentPrice)
                .Where(p => p != 0) 
                .Select(p => (double)p)
                .ToArray();


                var returns = Enumerable.Range(1, prices.Length - 1)
                    .Select(i => (prices[i] - prices[i - 1]) / prices[i - 1])
                    .Where(r => !double.IsNaN(r) && !double.IsInfinity(r))
                    .Select(r => (double?)r) 
                    .ToList();

                var stdDev = ExcelHelper.ComputeStandardDeviation(returns);



                ws.Cells[row, 1].Value = grp.Key;
                ws.Cells[row, 2].Value = stdDev;
                row++;
            }

            pkg.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
