using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Utils;
using Microsoft.EntityFrameworkCore;

using ClosedXML.Excel;



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
                        using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Volatility");

            ws.Cell(1, 1).Value = "Symbol";
            ws.Cell(1, 2).Value = "StdDev";
            ws.Row(1).Style.Font.SetBold();


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



                ws.Cell(row, 1).Value = grp.Key;
                ws.Cell(row, 2).Value = stdDev;
                row++;
            }

            wb.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
