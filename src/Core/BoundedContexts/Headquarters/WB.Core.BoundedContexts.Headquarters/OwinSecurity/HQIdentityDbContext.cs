using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using DbConfiguration = WB.Core.BoundedContexts.Headquarters.Repositories.DbConfiguration;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HQIdentityDbContext : DbContext
    {
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


        public HQIdentityDbContext() : base("Postgres")
        {
           
        }

        private const string SchemaName = "users";

        public virtual IDbSet<DeviceSyncInfo> DeviceSyncInfos { get; set; }

        // Here you define your own DbSet's
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Needed to ensure subclasses share the same table
            var user = modelBuilder.Entity<HqUser>()
                .HasKey(x => x.Id)
                .ToTable("users", SchemaName);

            user.HasMany(u => u.Roles).WithRequired().HasForeignKey(ur => ur.UserId);
            user.HasMany(u => u.Claims).WithRequired().HasForeignKey(uc => uc.UserId);
            user.HasMany(u => u.Logins).WithRequired().HasForeignKey(ul => ul.UserId);
            user.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UserNameIndex") { IsUnique = true }));

            // CONSIDER: u.Email is Required if set on options?
            user.Property(u => u.Email).HasMaxLength(256);

            modelBuilder.Entity<HqUserRole>()
                .HasKey(r => new {r.UserId, r.RoleId})
                .ToTable("userroles", SchemaName);

            modelBuilder.Entity<HqUserLogin>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                .ToTable("userlogins", SchemaName);

            modelBuilder.Entity<HqUserClaim>()
                .HasKey(x => x.Id)
                .ToTable("userclaims", SchemaName);

            var role = modelBuilder.Entity<HqRole>()
                .HasKey(x => x.Id)
                .ToTable("roles", SchemaName);

            role.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("RoleNameIndex") { IsUnique = true }));
            role.HasMany(r => r.Users).WithRequired().HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<HqUserProfile>().ToTable("userprofiles", SchemaName);
            modelBuilder.Entity<DeviceSyncInfo>().ToTable("devicesyncinfo", DbConfiguration.SchemaName);
            modelBuilder.Entity<SyncStatistics>().ToTable("devicesyncstatistics", DbConfiguration.SchemaName);
        }

        /// <summary>
        ///     Validates that UserNames are unique and case insenstive
        /// </summary>
        /// <param name="entityEntry"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry,
            IDictionary<object, object> items)
        {
            if (entityEntry != null && entityEntry.State == EntityState.Added)
            {
                var errors = new List<DbValidationError>();
                //check for uniqueness of user name and email
                if (entityEntry.Entity is HqUser user)
                {
                    if (Users.Any(u => string.Equals(u.UserName, user.UserName)))
                    {
                        errors.Add(new DbValidationError("User",
                            string.Format(CultureInfo.CurrentCulture, "User name {0} is already taken.", user.UserName)));
                    }
                    if (RequireUniqueEmail && Users.Any(u => string.Equals(u.Email, user.Email)))
                    {
                        errors.Add(new DbValidationError("User",
                            string.Format(CultureInfo.CurrentCulture, "Email {0} is already taken.", user.Email)));
                    }
                }
                else
                {
                    //check for uniqueness of role name
                    if (entityEntry.Entity is HqRole role && Roles.Any(r => string.Equals(r.Name, role.Name)))
                    {
                        errors.Add(new DbValidationError("Role",
                            string.Format(CultureInfo.CurrentCulture, "Role {0} already exists.", role.Name)));
                    }
                }
                if (errors.Any())
                {
                    return new DbEntityValidationResult(entityEntry, errors);
                }
            }
            return base.ValidateEntity(entityEntry, items);
        }
    }
}
