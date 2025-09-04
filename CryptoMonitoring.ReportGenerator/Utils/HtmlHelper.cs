using System.Text;

namespace CryptoMonitoring.ReportGenerator.Utils
{
    public class HtmlHelper
    {
        public static void SaveDataToHtml(
            IEnumerable<decimal> data,
            int smaPeriod,
            string filePath)
        {
            var prices = data.Select(d => (double)d).ToList();
            var sma = ExcelHelper.ComputeSMA(data, smaPeriod);

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ru\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"utf-8\" />");
            sb.AppendLine("  <title>Отчет по ценам</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("    th, td { border: 1px solid #ccc; padding: 8px; text-align: right; }");
            sb.AppendLine("    th { background: #f4f4f4; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <h2>Цены и скользящая средняя (SMA" + smaPeriod + ")</h2>");
            sb.AppendLine("  <table>");
            sb.AppendLine("    <thead>");
            sb.AppendLine("      <tr>");
            sb.AppendLine("        <th>Дата</th>");
            sb.AppendLine("        <th>Цена</th>");
            sb.AppendLine("        <th>SMA" + smaPeriod + "</th>");
            sb.AppendLine("      </tr>");
            sb.AppendLine("    </thead>");
            sb.AppendLine("    <tbody>");

            for (int i = 0; i < prices.Count; i++)
            {
                var date = DateTime.UtcNow.Date.AddDays(i - prices.Count + 1) 
                            .ToString("yyyy-MM-dd");
                var price = prices[i].ToString("F2");
                var smaVal = sma[i].ToString("F2");

                sb.AppendLine("      <tr>");
                sb.AppendLine($"        <td>{date}</td>");
                sb.AppendLine($"        <td>{price}</td>");
                sb.AppendLine($"        <td>{smaVal}</td>");
                sb.AppendLine("      </tr>");
            }

            sb.AppendLine("    </tbody>");
            sb.AppendLine("  </table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
