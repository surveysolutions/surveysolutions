using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HQIdentityDbContext : IdentityDbContext<HqUser, HqRole, Guid, HqUserLogin, HqUserRole, HqUserClaim>
    {
        public HQIdentityDbContext() : base("Postgres")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HQIdentityDbContext, HQIdentityDbMigrationsConfiguration>());
        }

        // Here you define your own DbSet's
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HqUser>().ToTable("users", HQIdentityDbMigrationsConfiguration.SchemaName);
            modelBuilder.Entity<HqRole>().ToTable("roles", HQIdentityDbMigrationsConfiguration.SchemaName);
            modelBuilder.Entity<HqUserRole>().ToTable("userroles", HQIdentityDbMigrationsConfiguration.SchemaName);
            modelBuilder.Entity<HqUserLogin>().ToTable("userlogins", HQIdentityDbMigrationsConfiguration.SchemaName);
            modelBuilder.Entity<HqUserClaim>().ToTable("userclaims", HQIdentityDbMigrationsConfiguration.SchemaName);
            modelBuilder.Entity<HqUserProfile>().ToTable("userprofiles", HQIdentityDbMigrationsConfiguration.SchemaName);
        }
    }
}