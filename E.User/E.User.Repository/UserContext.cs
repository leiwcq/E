using E.Data.MySql;
using Microsoft.EntityFrameworkCore;

namespace E.User.Repository
{
    public class UserContext : MySqlContextBase
    {
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.Account> Accounts { get; set; }

        public DbSet<Models.AccountChangeLog> AccountChangeLogs { get; set; }
        public DbSet<Models.Scheme> Schemes { get; set; }
        public DbSet<Models.SchemeGatewayLog> SchemeGatewayLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.User>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
            });

            modelBuilder.Entity<Models.Account>(entity =>
            {
                entity.HasIndex(e => e.AccountId).IsUnique();
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<Models.AccountChangeLog>(entity =>
            {
                entity.HasIndex(e => e.LogId).IsUnique();
                entity.HasIndex(e => e.AccountId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.OperatorTime });
            });

            modelBuilder.Entity<Models.Scheme>(entity =>
            {
                entity.HasIndex(e => e.SchemeIndex).IsUnique();
                entity.HasIndex(e => e.SchemeId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.GameId });
                entity.HasIndex(e => new { e.UserId, e.GameId, e.Status, e.CreateTime });
                entity.HasIndex(e => new { e.UserId, e.GameId, e.Status, e.PaymentStatus, e.CreateTime });
            });

            modelBuilder.Entity<Models.SchemeGatewayLog>(entity =>
            {
                entity.HasIndex(e => e.GatewayLogId).IsUnique();
                entity.HasIndex(e => e.SchemeIndex);
                entity.HasIndex(e => e.GatewayName);
                entity.HasIndex(e => new { e.GatewayName, e.CreateTime });
            });
        }
    }
}