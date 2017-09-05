using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using DbConfiguration = WB.Core.BoundedContexts.Headquarters.Repositories.DbConfiguration;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HQIdentityDbContext : IdentityDbContext<HqUser, HqRole, Guid, HqUserLogin, HqUserRole, HqUserClaim>
    {
        public HQIdentityDbContext() : base("Postgres")
        {
           
        }

        private const string SchemaName = "users";

        public virtual IDbSet<DeviceSyncInfo> DeviceSyncInfos { get; set; }

        // Here you define your own DbSet's
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HqUser>().ToTable("users", SchemaName);
            modelBuilder.Entity<HqRole>().ToTable("roles", SchemaName);
            modelBuilder.Entity<HqUserRole>().ToTable("userroles", SchemaName);
            modelBuilder.Entity<HqUserLogin>().ToTable("userlogins", SchemaName);
            modelBuilder.Entity<HqUserClaim>().ToTable("userclaims", SchemaName);
            modelBuilder.Entity<HqUserProfile>().ToTable("userprofiles", SchemaName);

            modelBuilder.Entity<DeviceSyncInfo>().ToTable("devicesyncinfo", DbConfiguration.SchemaName);
            modelBuilder.Entity<SyncStatistics>().ToTable("devicesyncstatistics", DbConfiguration.SchemaName);
        }
    }
}