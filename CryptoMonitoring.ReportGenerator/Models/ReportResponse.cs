namespace CryptoMonitoring.ReportGenerator.Models
{
    public class ReportResponse
    {
        public Stream Stream { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public string FileName { get; set; } = default!;
    }
}
