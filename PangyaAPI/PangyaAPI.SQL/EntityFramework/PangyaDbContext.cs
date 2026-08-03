using Microsoft.EntityFrameworkCore;
using PangyaAPI.SQL.EntityFramework.Entities;

namespace PangyaAPI.SQL.EntityFramework
{
    public sealed class PangyaDbContext : DbContext
    {
        public PangyaDbContext(DbContextOptions<PangyaDbContext> options)
            : base(options)
        {
        }

        public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
        public DbSet<AuthServerKeyEntity> AuthServerKeys => Set<AuthServerKeyEntity>();
        public DbSet<AuthKeyLoginEntity> AuthKeyLogins => Set<AuthKeyLoginEntity>();
        public DbSet<IpBanEntity> IpBans => Set<IpBanEntity>();
        public DbSet<MacBanEntity> MacBans => Set<MacBanEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PangyaDbContext).Assembly);

            modelBuilder.Entity<AccountEntity>(entity =>
            {
                entity.ToTable("account", "pangya");
                entity.HasKey(value => value.Uid);
                entity.Property(value => value.Uid).HasColumnName("UID").ValueGeneratedNever();
                entity.Property(value => value.Logon).HasColumnName("LOGON");
                entity.Property(value => value.GameServerId).HasColumnName("game_server_id");
            });

            modelBuilder.Entity<AuthServerKeyEntity>(entity =>
            {
                entity.ToTable("pangya_auth_key", "pangya");
                entity.HasKey(value => value.ServerUid);
                entity.Property(value => value.ServerUid).HasColumnName("server_uid").ValueGeneratedNever();
                entity.Property(value => value.Key).HasColumnName("key");
                entity.Property(value => value.Valid).HasColumnName("VALID");
            });

            modelBuilder.Entity<AuthKeyLoginEntity>(entity =>
            {
                entity.ToTable("authkey_login", "pangya");
                entity.HasKey(value => value.Uid);
                entity.Property(value => value.Uid).HasColumnName("UID").ValueGeneratedNever();
                entity.Property(value => value.Valid).HasColumnName("valid");
            });

            modelBuilder.Entity<IpBanEntity>(entity =>
            {
                entity.ToTable("pangya_ip_table", "pangya");
                entity.HasNoKey();
                entity.Property(value => value.Ip).HasColumnName("ip");
                entity.Property(value => value.Mask).HasColumnName("mask");
            });

            modelBuilder.Entity<MacBanEntity>(entity =>
            {
                entity.ToTable("pangya_mac_table", "pangya");
                entity.HasNoKey();
                entity.Property(value => value.Mac).HasColumnName("mac");
            });
        }
    }
}
