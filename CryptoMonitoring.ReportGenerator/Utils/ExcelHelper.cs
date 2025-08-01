using Microsoft.EntityFrameworkCore;
using static OfficeOpenXml.ExcelErrorValue;

namespace CryptoMonitoring.ReportGenerator.Utils
{
    public class ExcelHelper
    {
        public static List<double> ComputeSMA(IEnumerable<decimal> data, int period)
        {
            var prices = new List<double>();
            foreach (var d in data) prices.Add((double)d);
            var sma = new List<double>();
            for (int i = 0; i < prices.Count; i++)
            {
                if (i + 1 < period)
                {
                    sma.Add(0);
                    continue;
                }
                var sum = 0.0;
                for (int j = i + 1 - period; j <= i; j++)
                    sum += prices[j];
                sma.Add(sum / period);
            }
            return sma;
        }

        public static double ComputeStandardDeviation(IEnumerable<double?> values)
        {

            var clean = values.Where(v => v.HasValue).Select(v => v.Value).ToList();
            if (clean.Count == 0) return 0;

            var avg = clean.Average();
            var sum = clean.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sum / clean.Count);
        }

    }
}
