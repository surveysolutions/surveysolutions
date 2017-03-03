using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HQIdentityDbContext : IdentityDbContext<ApplicationUser, AppRole, Guid, AppUserLogin, AppUserRole, AppUserClaim>
    {
        public HQIdentityDbContext() : base(@"DefaultConnection")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HQIdentityDbContext, Configuration>());
        }

        // Here you define your own DbSet's

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("users", OwinSecurity.Configuration.SchemaName);
            modelBuilder.Entity<AppRole>().ToTable("roles", OwinSecurity.Configuration.SchemaName);
            modelBuilder.Entity<AppUserRole>().ToTable("userroles", OwinSecurity.Configuration.SchemaName);
            modelBuilder.Entity<AppUserLogin>().ToTable("userlogins", OwinSecurity.Configuration.SchemaName);
            modelBuilder.Entity<AppUserClaim>().ToTable("userclaims", OwinSecurity.Configuration.SchemaName);
        }

        public static HQIdentityDbContext Create() => new HQIdentityDbContext();
    }
}