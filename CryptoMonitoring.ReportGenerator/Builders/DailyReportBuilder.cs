//using ClosedXML.Excel;
//using CryptoMonitoring.ReportGenerator.Builders;
//using CryptoMonitoring.ReportGenerator.Data;
//using CryptoMonitoring.ReportGenerator.Models;
//using Microsoft.EntityFrameworkCore;
//using System.IO;
//using System.Threading.Tasks;

//public class DailyReportBuilder : IReportBuilder
//{
//    public ReportType Type => ReportType.Daily;
//    private readonly ReportDbContext _db;

//    public DailyReportBuilder(ReportDbContext db) => _db = db;

//    public async Task<MemoryStream> BuildAsync(ReportRequest request)
//    {
//        if (request.Date == null)
//            throw new ArgumentException("Date is required");

//        var date = request.Date.Value.Date;
//        var next = date.AddDays(1);

//        var data = await _db.Snapshots.AsNoTracking()

//            //.Where(s => EF.Functions.DateDiffDay(date, s.Timestamp) == 0)
//            //.Where(s => s.Timestamp >= date && s.Timestamp < next)
//            .OrderBy(s => s.Timestamp)
//            .ToListAsync();

//        using var workbook = new XLWorkbook();
//        var ws = workbook.Worksheets.Add("Daily Report");

//        ws.Cell(1, 1).Value = $"Rows: {data.Count}";
//        ws.Row(1).Style.Font.SetItalic();

//        ws.Cell(2, 1).Value = "Timestamp";
//        ws.Cell(2, 2).Value = "Symbol";
//        ws.Cell(2, 3).Value = "Price";
//        ws.Range(2, 1, 2, 3)
//          .Style
//          .Font.SetBold()
//          .Fill.SetBackgroundColor(XLColor.LightGray);

//        var row = 3;
//        foreach (var item in data)
//        {
//            ws.Cell(row, 1).Value = item.Timestamp;
//            ws.Cell(row, 1).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
//            ws.Cell(row, 2).Value = item.Symbol;
//            ws.Cell(row, 3).Value = (double)item.CurrentPrice;
//            row++;
//        }

//        ws.Columns(1, 3).AdjustToContents();
//        if (data.Count > 0)
//            ws.Range(2, 1, row - 1, 3).AsTable()
//              .Theme = XLTableTheme.TableStyleMedium2;

//        var ms = new MemoryStream();
//        workbook.SaveAs(ms);
//        ms.Position = 0;
//        return ms;
//    }

//}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CryptoMonitoring.ReportGenerator.Builders;
using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Models;
using Microsoft.EntityFrameworkCore;

using DocumentFormat.OpenXml.Packaging;
using SpreadsheetModel = DocumentFormat.OpenXml.Spreadsheet;
using DW = DocumentFormat.OpenXml.Drawing;
using DWSP = DocumentFormat.OpenXml.Drawing.Spreadsheet;
using ChartDrawing = DocumentFormat.OpenXml.Drawing.Charts;

public class DailyReportBuilder : IReportBuilder
{
    public ReportType Type => ReportType.Daily;
    private readonly ReportDbContext _db;

    public DailyReportBuilder(ReportDbContext db) => _db = db;

    public async Task<MemoryStream> BuildAsync(ReportRequest request)
    {
        if (request.Date == null)
            throw new ArgumentException("Date is required", nameof(request.Date));

        var date = request.Date.Value.Date;
        var nextDate = date.AddDays(1);
        var prevDate = date.AddDays(-1);

        var todayData = await _db.Snapshots
            .AsNoTracking()
            .Where(s => s.Timestamp >= date && s.Timestamp < nextDate)
            .ToListAsync();

        var yesterdayClose = await _db.Snapshots
            .AsNoTracking()
            .Where(s => s.Timestamp >= prevDate && s.Timestamp < date)
            .GroupBy(s => s.Symbol)
            .Select(g => new {
                Symbol = g.Key,
                ClosePrev = g.OrderBy(x => x.Timestamp).Last().CurrentPrice
            })
            .ToDictionaryAsync(x => x.Symbol, x => x.ClosePrev);

        var reportRows = todayData
            .GroupBy(s => s.Symbol)
            .Select(g =>
            {
                var last = g.OrderBy(x => x.Timestamp).Last();
                var prevClose = yesterdayClose.ContainsKey(last.Symbol)
                    ? yesterdayClose[last.Symbol]
                    : last.CurrentPrice;

                var absChange = last.CurrentPrice - prevClose;
                var pctChange = prevClose == 0
                    ? 0
                    : (double)(absChange / prevClose * 100);

                return new
                {
                    last.Symbol,
                    ClosePrice = (double)last.CurrentPrice,
                    Change24h = last.Change24h,
                    AbsChange = (double)absChange,
                    PctChange = Math.Round(pctChange, 2),
                    SMA7 = (double?)last.Sma7,
                    SMA21 = (double?)last.Sma21,
                    Support = (double?)last.SupportLevel,
                    Resistance = (double?)last.ResistanceLevel,
                    Volatility = last.Volatility
                };
            })
            .OrderBy(r => r.Symbol)
            .ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Daily Report");

        ws.Cell(1, 1).Value = $"Daily Report: {date:yyyy-MM-dd}";
        ws.Range(1, 1, 1, 10)
          .Merge()
          .Style.Font.SetBold()
                   .Font.FontSize = 16;

        var headers = new[] {
            "Symbol", "ClosePrice", "Change24h", "AbsChange", "%Change",
            "SMA7", "SMA21", "Support", "Resistance", "Volatility"
        };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(3, i + 1).Value = headers[i];

        ws.Range(3, 1, 3, headers.Length)
          .Style.Font.SetBold()
                   .Fill.SetBackgroundColor(XLColor.LightGray);

        for (int i = 0; i < reportRows.Count; i++)
        {
            var row = 4 + i;
            var r = reportRows[i];

            ws.Cell(row, 1).Value = r.Symbol;
            ws.Cell(row, 2).Value = r.ClosePrice;
            ws.Cell(row, 3).Value = r.Change24h;
            ws.Cell(row, 4).Value = r.AbsChange;
            ws.Cell(row, 5).Value = r.PctChange;
            ws.Cell(row, 6).Value = r.SMA7;
            ws.Cell(row, 7).Value = r.SMA21;
            ws.Cell(row, 8).Value = r.Support;
            ws.Cell(row, 9).Value = r.Resistance;
            ws.Cell(row, 10).Value = r.Volatility;
        }

        ws.Columns(1, headers.Length).AdjustToContents();
        ws.Range(3, 1, 3 + reportRows.Count, headers.Length)
          .AsTable()
          .Theme = XLTableTheme.TableStyleMedium9;

        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;

        using (var document = SpreadsheetDocument.Open(ms, true))
        {
            var wbPart = document.WorkbookPart!;
            var sheet = wbPart.Workbook
                .Sheets
                .Cast<SpreadsheetModel.Sheet>()
                .First(s => s.Name == "Daily Report");
            var wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id!);

            var drPart = wsPart.DrawingsPart ?? wsPart.AddNewPart<DrawingsPart>();
            drPart.WorksheetDrawing ??= new DWSP.WorksheetDrawing();

            var chartPart = drPart.AddNewPart<ChartPart>();
            GenerateLineChart(chartPart, reportRows.Count);

            var from = new DWSP.FromMarker(
                new DWSP.ColumnId("1"), new DWSP.ColumnOffset("0"),
                new DWSP.RowId((5 + reportRows.Count).ToString()), new DWSP.RowOffset("0")
            );
            var to = new DWSP.ToMarker(
                new DWSP.ColumnId("8"), new DWSP.ColumnOffset("0"),
                new DWSP.RowId((5 + reportRows.Count + 15).ToString()), new DWSP.RowOffset("0")
            );
            var gf = new DWSP.GraphicFrame(
                new DWSP.NonVisualGraphicFrameProperties(
                    new DWSP.NonVisualDrawingProperties { Id = 2U, Name = "PriceTrend" },
                    new DWSP.NonVisualGraphicFrameDrawingProperties()
                ),
                new DW.Transform2D(
                    new DW.Offset { X = 0, Y = 0 },
                    new DW.Extents { Cx = 0, Cy = 0 }
                ),
                new DW.Graphic(
                    new DW.GraphicData(
                        new ChartDrawing.ChartReference { Id = drPart.GetIdOfPart(chartPart) }
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }
                )
            );
            var anchor = new DWSP.TwoCellAnchor(from, to, gf, new DWSP.ClientData())
            {
                EditAs = DWSP.EditAsValues.OneCell
            };

            drPart.WorksheetDrawing.Append(anchor);
            drPart.WorksheetDrawing.Save();
        }

        ms.Position = 0;
        return ms;
    }

    private void GenerateLineChart(ChartPart chartPart, int rowCount)
    {
        var chartSpace = new ChartDrawing.ChartSpace();
        var chart = new ChartDrawing.Chart();
        chart.Append(new ChartDrawing.AutoTitleDeleted { Val = true });

        var plotArea = new ChartDrawing.PlotArea();
        plotArea.Append(new ChartDrawing.Layout());

        var lineChart = new ChartDrawing.LineChart(
            new ChartDrawing.Grouping { Val = ChartDrawing.GroupingValues.Standard }
        );

        // Одна серия: цены закрытия
        var series = new ChartDrawing.LineChartSeries(
            new ChartDrawing.Index { Val = 0U },
            new ChartDrawing.Order { Val = 0U },
            new ChartDrawing.SeriesText(
                new ChartDrawing.NumericValue { Text = "Price Close" }
            ),
            new ChartDrawing.CategoryAxisData(
                new ChartDrawing.StringReference(
                    new ChartDrawing.Formula($"'Daily Report'!$A$4:$A${4 + rowCount - 1}")
                )
            ),
            new ChartDrawing.Values(
                new ChartDrawing.NumberReference(
                    new ChartDrawing.Formula($"'Daily Report'!$B$4:$B${4 + rowCount - 1}")
                )
            )
        );

        lineChart.Append(series);
        plotArea.Append(lineChart);

        chart.Append(plotArea);
        chart.Append(new ChartDrawing.PlotVisibleOnly { Val = true });
        chartSpace.Append(chart);

        chartPart.ChartSpace = chartSpace;
    }
}
