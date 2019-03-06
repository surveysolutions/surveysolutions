using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerIdentityUser : IdentityUser
    {

    }

    public class DesignerDbContext : IdentityDbContext<DesignerIdentityUser>
    {
    }
}
