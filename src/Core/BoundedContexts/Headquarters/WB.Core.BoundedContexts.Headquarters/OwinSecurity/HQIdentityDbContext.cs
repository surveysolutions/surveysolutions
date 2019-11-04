using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HQIdentityDbContext : DbContext
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;

        /// <summary>
        ///     IDbSet of Roles
        /// </summary>
        public virtual DbSet<HqRole> Roles { get; set; }
        /// <summary>
        ///     IDbSet of Users
        /// </summary>
        public virtual DbSet<HqUser> Users { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of User roles.
        /// </summary>
        public virtual DbSet<HqUserRole> UserRoles { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of User claims.
        /// </summary>
        public DbSet<HqUserClaim> UserClaims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of User logins.
        /// </summary>
        public DbSet<HqUserLogin> UserLogins { get; set; }
        /// <summary>
        ///     If true validates that emails are unique
        /// </summary>
        public bool RequireUniqueEmail { get; set; }


        public HQIdentityDbContext(UnitOfWorkConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.UseNpgsql(this.connectionSettings.ConnectionString);

        private const string SchemaName = "users";

        public virtual DbSet<DeviceSyncInfo> DeviceSyncInfos { get; set; }

        // Here you define your own DbSet's
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HqUser>(b =>
            {
                // Primary key
                b.HasKey(u => u.Id);

                // Indexes for "normalized" username and email, to allow efficient lookups
                b.HasIndex(u => u.UserName).HasName("UserNameIndex").IsUnique();
                b.HasIndex(u => u.Email).HasName("EmailIndex");

                // Maps to the AspNetUsers table
                b.ToTable("users", SchemaName);
                
                // Limit the size of columns to use efficient database types
                b.Property(u => u.UserName).HasMaxLength(256);
                b.Property(u => u.Email).HasMaxLength(256);

                // The relationships between User and other entity types
                // Note that these relationships are configured with no navigation properties

                // Each User can have many UserClaims
                b.HasMany(x => x.Claims).WithOne().HasForeignKey(uc => uc.UserId).IsRequired();

                // Each User can have many UserLogins
                b.HasMany(x => x.Logins).WithOne().HasForeignKey(ul => ul.UserId).IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(x => x.Roles).WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            });

            builder.Entity<HqUserClaim>(b =>
            {
                // Primary key
                b.HasKey(uc => uc.Id);

                // Maps to the AspNetUserClaims table
                b.ToTable("userclaims", SchemaName);
            });

            builder.Entity<HqUserLogin>(b =>
            {
                // Composite primary key consisting of the LoginProvider and the key to use
                // with that provider
                b.HasKey(l => new {l.LoginProvider, l.ProviderKey});

                // Limit the size of the composite key columns due to common DB restrictions
                b.Property(l => l.LoginProvider).HasMaxLength(128);
                b.Property(l => l.ProviderKey).HasMaxLength(128);

                // Maps to the AspNetUserLogins table
                b.ToTable("userlogins", SchemaName);
            });

            builder.Entity<HqRole>(b =>
            {
                // Primary key
                b.HasKey(r => r.Id);

                // Index for "normalized" role name to allow efficient lookups
                b.HasIndex(r => r.Name).HasName("RoleNameIndex").IsUnique();

                // Maps to the AspNetRoles table
                b.ToTable("roles", SchemaName);

                // Limit the size of columns to use efficient database types
                b.Property(u => u.Name).HasMaxLength(256).IsRequired();

                // The relationships between Role and other entity types
                // Note that these relationships are configured with no navigation properties

                // Each Role can have many entries in the UserRole join table
                b.HasMany(x => x.Users).WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();
            });

            builder.Entity<HqUserRole>(b =>
            {
                // Primary key
                b.HasKey(r => new {r.UserId, r.RoleId});

                // Maps to the AspNetUserRoles table
                b.ToTable("userroles", SchemaName);
            });
            
            builder.Entity<HqUserProfile>().ToTable("userprofiles", SchemaName);
            builder.Entity<DeviceSyncInfo>(b =>
            {
                b.HasIndex(dsi => new {dsi.InterviewerId, dsi.AndroidSdkVersion, dsi.AppBuildVersion})
                    .HasName("devicesyncinfo_interviewerid_androidsdkversion_appbuildversion").IsUnique(false);

                b.ToTable("devicesyncinfo", DbConfiguration.SchemaName);
            });
            builder.Entity<SyncStatistics>().ToTable("devicesyncstatistics", DbConfiguration.SchemaName);
        }
    }
}
