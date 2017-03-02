using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Migrations;

namespace WB.UI.Headquarters.OwinSecurity
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

            modelBuilder.Entity<ApplicationUser>().ToTable("users", Migrations.Configuration.SchemaName);
            modelBuilder.Entity<AppRole>().ToTable("roles", Migrations.Configuration.SchemaName);
            modelBuilder.Entity<AppUserRole>().ToTable("userroles", Migrations.Configuration.SchemaName);
            modelBuilder.Entity<AppUserLogin>().ToTable("userlogins", Migrations.Configuration.SchemaName);
            modelBuilder.Entity<AppUserClaim>().ToTable("userclaims", Migrations.Configuration.SchemaName);
        }

        public static HQIdentityDbContext Create() => new HQIdentityDbContext();
    }
}