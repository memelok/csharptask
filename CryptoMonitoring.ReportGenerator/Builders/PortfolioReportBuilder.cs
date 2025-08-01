using System.ComponentModel;
using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Models;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;


namespace CryptoMonitoring.ReportGenerator.Builders
{
    public class PortfolioReportBuilder : IReportBuilder
    {
        public ReportType Type => ReportType.Portfolio;

        private readonly ReportDbContext _db;

        public PortfolioReportBuilder(ReportDbContext db) => _db = db;

        public async Task<MemoryStream> BuildAsync(ReportRequest request)
        {

            if (request.CustomParams == null || !request.CustomParams.TryGetValue("PortfolioId", out var pid))
                throw new ArgumentException("PortfolioId is required");

            // 
            // по Symbol
            var data = await _db.Snapshots
                .Where(s => s.Timestamp.Date == DateTime.UtcNow.Date)
                .ToListAsync();

            var ms = new MemoryStream();

            ExcelPackage.License.SetNonCommercialPersonal("CryptoMonitoring");
            using var pkg = new ExcelPackage();
            var ws = pkg.Workbook.Worksheets.Add("Portfolio");

            ws.Cells["A1"].Value = "Symbol";
            ws.Cells["B1"].Value = "Allocation%";
            ws.Cells["C1"].Value = "CurrentValue";

            var symbols = data.Select(d => d.Symbol).Distinct().ToArray();
            var equalPct = 1m / symbols.Length;
            int row = 2;
            foreach (var sym in symbols)
            {
                var price = data.First(d => d.Symbol == sym).CurrentPrice;
                ws.Cells[row, 1].Value = sym;
                ws.Cells[row, 2].Value = (double)equalPct * 100;
                ws.Cells[row, 3].Value = (double)(price * equalPct);
                row++;
            }

            pkg.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
