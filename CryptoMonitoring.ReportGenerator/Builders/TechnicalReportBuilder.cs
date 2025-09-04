using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Models;
using CryptoMonitoring.ReportGenerator.Utils;

using ClosedXML.Excel;

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
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Technical Analysis");

            ws.Cell(1, 1).Value = "Timestamp";
            ws.Cell(1, 2).Value = "Symbol";
            ws.Cell(1, 3).Value = "ClosePrice";
            ws.Cell(1, 4).Value = "SMA(10)";
            ws.Row(1).Style.Font.SetBold();

            int row = 2;
            foreach (var grp in data.GroupBy(x => x.Symbol))
            {
                var prices = grp.Select(x => x.CurrentPrice).ToList();
                var sma = ExcelHelper.ComputeSMA(prices, period: 10);

                for (int i = 0; i < grp.Count(); i++)
                {
                    var item = grp.ElementAt(i);
                    ws.Cell(row, 1).Value = item.Timestamp;
                    ws.Cell(row, 2).Value = item.Symbol;
                    ws.Cell(row, 3).Value = (double)item.CurrentPrice;
                    ws.Cell(row, 4).Value = (i < sma.Count ? (double?)sma[i] : null);
                    row++;
                }
            }

            wb.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
    
}
}
