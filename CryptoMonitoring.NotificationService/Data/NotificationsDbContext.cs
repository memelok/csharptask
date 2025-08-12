using CryptoMonitoring.NotificationService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;


namespace CryptoMonitoring.NotificationService.Data
{
    public class NotificationsDbContext : DbContext
    {
        public NotificationsDbContext(DbContextOptions<NotificationsDbContext> opts)
        : base(opts) { }

        public DbSet<NotificationSubscription> Subscriptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NotificationSubscription>()
                .HasKey(s => s.Id);
            builder.Entity<NotificationSubscription>()
                .HasIndex(s => new { s.EventType, s.IsActive });
        }
    }
}
