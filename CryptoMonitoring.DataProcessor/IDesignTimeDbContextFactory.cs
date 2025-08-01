using CryptoMonitoring.DataProcessor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CryptoMonitoring.DataProcessor
{
    public class IDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PostgresDbContext>
    {
        public PostgresDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PostgresDbContext>();
            var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=postgres;Port=5432;Database=db;Username=user;Password=pass";
            optionsBuilder.UseNpgsql(connStr);

            return new PostgresDbContext(optionsBuilder.Options);
        }
    }
}
