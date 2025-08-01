using Microsoft.EntityFrameworkCore;
using CryptoMonitoring.ReportGenerator.Models;

namespace CryptoMonitoring.ReportGenerator.Data
{
    public class ReportDbContext : DbContext
    {
        public DbSet<Snapshot> Snapshots { get; set; } = null!;

        public ReportDbContext(DbContextOptions<ReportDbContext> options)
        : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Snapshot>(b =>
            {
                b.ToTable("Snapshots");
                b.HasKey(x => x.Id);
                b.Property(x => x.Symbol).HasMaxLength(16).IsRequired();
                b.Property(x => x.Name).HasMaxLength(64).IsRequired();
                b.Property(x => x.CurrentPrice).HasColumnType("numeric(18,8)");
                b.Property(x => x.PreviousPrice).HasColumnType("numeric(18,8)");
                b.Property(x => x.Change24h).HasColumnType("double precision");
                b.Property(x => x.Timestamp).IsRequired();
            });
        }
    }
}
