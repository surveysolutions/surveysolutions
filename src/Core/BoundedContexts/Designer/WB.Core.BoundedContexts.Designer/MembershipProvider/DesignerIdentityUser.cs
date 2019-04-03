using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerIdentityUser : IdentityUser
    {

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
        }
    }
}
