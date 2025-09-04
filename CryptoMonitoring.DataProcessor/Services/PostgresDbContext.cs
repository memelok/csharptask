using Microsoft.EntityFrameworkCore;
using CryptoMonitoring.DataProcessor.Models;

namespace CryptoMonitoring.DataProcessor.Services
{
    public class PostgresDbContext : DbContext
    {
        public DbSet<EnrichedMarket> Snapshots { get; set; } = null!;

        public PostgresDbContext(DbContextOptions<PostgresDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnrichedMarket>(b =>
            {
                b.ToTable("Snapshots");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("Id");
                b.Property(x => x.Symbol).HasColumnName("Symbol");
                b.Property(x => x.Name).HasColumnName("Name");
                b.Property(x => x.CurrentPrice).HasColumnName("CurrentPrice");
                b.Property(x => x.Change24h).HasColumnName("Change24h");
                b.Property(x => x.Timestamp).HasColumnName("Timestamp");
                b.Property(x => x.Sma7).HasColumnName("Sma7");
                b.Property(x => x.Sma21).HasColumnName("Sma21");
                b.Property(x => x.SupportLevel).HasColumnName("SupportLevel");
                b.Property(x => x.ResistanceLevel).HasColumnName("ResistanceLevel");
                b.Property(x => x.Volatility).HasColumnName("Volatility");
            });
        }
    }
}
