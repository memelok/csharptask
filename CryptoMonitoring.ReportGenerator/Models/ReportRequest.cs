namespace CryptoMonitoring.ReportGenerator.Models
{
    public class ReportRequest
    {
        public ReportType Type { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string[]? Symbols { get; set; }
        public Dictionary<string, string>? CustomParams { get; set; }
    }
}
