using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoMonitoring.ReportGenerator.Utils
{
    public class ExcelHelper
    {
        public static List<double> 
            ComputeSMA(IEnumerable<decimal> data, int period) 
        { 
            var prices = data.Select(d => (double)d).ToList();
            var sma = new List<double>();
            for (int i = 0; i < prices.Count; i++) 
            { 
                if (i + 1 < period) 
                {
                    sma.Add(0); 
                    continue; 
                } 
                
                double sum = 0; 
                for (int j = i + 1 - period; j <= i; j++) 
                    sum += prices[j]; 
                sma.Add(sum / period); 
            } 
            return sma; 
        }
        public static double ComputeStandardDeviation(IEnumerable<double?> values) 
        { 
            var clean = values.Where(v => v.HasValue).Select(v => v.Value).ToList();
            if (clean.Count == 0) 
                return 0;
            var avg = clean.Average();
            var sum = clean.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sum / clean.Count); 
        }
        public static void SaveDataToExcel(IEnumerable<decimal> data, string filePath) 
        {
            using var workbook = new XLWorkbook(); 
            var ws = workbook.Worksheets.Add("Report");
            ws.Cell(1, 1).Value = "Price"; 
            var list = data.ToList();
            for (int i = 0; i < list.Count; i++)
                ws.Cell(i + 2, 1).Value = (double)list[i];
            workbook.SaveAs(filePath); 
        }
    }
}
