using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoMonitoring.ReportGenerator.Models
{
    public class Snapshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Symbol { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal CurrentPrice { get; set; }
        public decimal PreviousPrice { get; set; }
        public double Change24h { get; set; }
        public DateTime Timestamp { get; set; }

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
