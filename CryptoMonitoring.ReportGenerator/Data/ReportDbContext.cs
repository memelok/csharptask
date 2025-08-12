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
                b.Property(x => x.Id)
                 .HasColumnName("Id");

                b.Property(x => x.Symbol)
                 .HasColumnName("Symbol");

                b.Property(x => x.Name)
                 .HasColumnName("Name");

                b.Property(x => x.CurrentPrice)
                 .HasColumnName("CurrentPrice");

                b.Property(x => x.PreviousPrice)
                 .HasColumnName("PreviousPrice");

                b.Property(x => x.Change24h)
                 .HasColumnName("Change24h");

                b.Property(x => x.Timestamp)
                 .HasColumnName("Timestamp");
            });
        }
    }
}
