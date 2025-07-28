using Microsoft.EntityFrameworkCore;
using CryptoMonitoring.DataProcessor.Models;
using System.Collections.Generic;
namespace CryptoMonitoring.DataProcessor.Services
{
    public class PostgresDbContext : DbContext
    {
        public PostgresDbContext(DbContextOptions<PostgresDbContext> opts)
        : base(opts) { }

        public DbSet<EnrichedMarket> Snapshots { get; set; }
    }
}
