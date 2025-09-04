using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace CryptoMonitoring.ReportGenerator.Builders
{
    public class PortfolioReportBuilder : IReportBuilder
    {
        public ReportType Type => ReportType.Portfolio;
        private readonly ReportDbContext _db;

        public PortfolioReportBuilder(ReportDbContext db) => _db = db;

        public async Task<MemoryStream> BuildAsync(ReportRequest request)
        {
            if (request.CustomParams == null
                || !request.CustomParams.TryGetValue("PortfolioId", out var pid))
                throw new ArgumentException("PortfolioId is required");

            var data = await _db.Snapshots
                .Where(s => s.Timestamp.Date == DateTime.UtcNow.Date)
                .ToListAsync();

            var ms = new MemoryStream();
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Portfolio");

            ws.Cell(1, 1).Value = "Symbol";
            ws.Cell(1, 2).Value = "Allocation%";
            ws.Cell(1, 3).Value = "CurrentValue";
            ws.Row(1).Style.Font.SetBold();

            var symbols = data.Select(d => d.Symbol).Distinct().ToArray();
            var equalPct = 1m / symbols.Length;
            int row = 2;

            foreach (var sym in symbols)
            {
                var price = data.First(d => d.Symbol == sym).CurrentPrice;
                ws.Cell(row, 1).Value = sym;
                ws.Cell(row, 2).Value = (double)equalPct * 100;
                ws.Cell(row, 3).Value = (double)(price * equalPct);
                row++;
            }

            wb.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
