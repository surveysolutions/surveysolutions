using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerIdentityUser : IdentityUser
    {
        public string PasswordSalt { get; set; }
    }

    public class DesignerDbContext : IdentityDbContext<DesignerIdentityUser>
    {
        public DesignerDbContext(DbContextOptions options) : base(options)
        {
        }

        protected DesignerDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("plainstore");

            builder.Entity<DesignerIdentityUser>(x => x.Property(p => p.PasswordSalt).HasColumnName("PasswordSalt"));
        }
    }
}
