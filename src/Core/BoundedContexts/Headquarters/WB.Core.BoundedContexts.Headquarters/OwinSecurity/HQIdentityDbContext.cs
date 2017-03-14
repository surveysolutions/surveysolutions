using System;
using System.Data.Entity;
using System.Web.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HQIdentityDbContext : IdentityDbContext<HqUser, HqRole, Guid, HqUserLogin, HqUserRole, HqUserClaim>
    {
        public HQIdentityDbContext() : base("Postgres")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HQIdentityDbContext, Configuration>());
        }

        // Here you define your own DbSet's

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HqUser>().ToTable("users", OwinSecurity.Configuration.SchemaName);
            modelBuilder.Entity<HqRole>().ToTable("roles", OwinSecurity.Configuration.SchemaName);
            modelBuilder.Entity<HqUserRole>().ToTable("userroles", OwinSecurity.Configuration.SchemaName);
            modelBuilder.Entity<HqUserLogin>().ToTable("userlogins", OwinSecurity.Configuration.SchemaName);
            modelBuilder.Entity<HqUserClaim>().ToTable("userclaims", OwinSecurity.Configuration.SchemaName);
        }

        public static HQIdentityDbContext Create() => new HQIdentityDbContext();
    }
}