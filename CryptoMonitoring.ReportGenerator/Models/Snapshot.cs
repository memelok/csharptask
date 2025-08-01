namespace CryptoMonitoring.ReportGenerator.Models
{
    public class Snapshot
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal CurrentPrice { get; set; }
        public decimal PreviousPrice { get; set; }
        public double Change24h { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
