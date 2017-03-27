﻿using System.Data.Entity;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public class HQPlainStorageDbContext : DbContext
    {
        public DbSet<DeviceSyncInfo> DeviceSyncInfo { get; set; }
        public DbSet<SyncStatistics> SyncStatistics { get; set; }

        public HQPlainStorageDbContext() { }

        public HQPlainStorageDbContext(string connectionStringName) : base(connectionStringName)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HQPlainStorageDbContext, DbConfiguration>(connectionStringName));
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DeviceSyncInfo>().ToTable("devicesyncinfo", DbConfiguration.SchemaName);
            modelBuilder.Entity<SyncStatistics>().ToTable("devicesyncstatistics", DbConfiguration.SchemaName);
        }
    }
}
