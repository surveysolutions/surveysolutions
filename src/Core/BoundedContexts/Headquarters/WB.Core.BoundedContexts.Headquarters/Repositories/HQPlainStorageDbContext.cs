using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public class HQPlainStorageDbContext : DbContext
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;
        public DbSet<DeviceSyncInfo> DeviceSyncInfo { get; set; }
        public DbSet<SyncStatistics> SyncStatistics { get; set; }
        
        public HQPlainStorageDbContext(UnitOfWorkConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql(this.connectionSettings.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DeviceSyncInfo>().ToTable("devicesyncinfo", DbConfiguration.SchemaName);
            modelBuilder.Entity<SyncStatistics>().ToTable("devicesyncstatistics", DbConfiguration.SchemaName);
        }
    }
}
