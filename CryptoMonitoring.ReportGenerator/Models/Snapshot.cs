using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoMonitoring.ReportGenerator.Models
{
    public class Snapshot
    {
        [Key]
        [Column("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("Symbol")]
        public string Symbol { get; set; } = null!;

        [Column("Name")]
        public string Name { get; set; } = null!;

        [Column("CurrentPrice")]
        public decimal CurrentPrice { get; set; }

        [Column("Change24h")]
        public double Change24h { get; set; }

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("Sma7")]
        public decimal? Sma7 { get; set; }

        [Column("Sma21")]
        public decimal? Sma21 { get; set; }

        [Column("SupportLevel")]
        public decimal? SupportLevel { get; set; }

        [Column("ResistanceLevel")]
        public decimal? ResistanceLevel { get; set; }

        [Column("Volatility")]
        public double? Volatility { get; set; }

        public Snapshot()
        {
            Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public void Normalize()
        {
            Timestamp = DateTime.SpecifyKind(Timestamp, DateTimeKind.Utc);
        }

    }
}
