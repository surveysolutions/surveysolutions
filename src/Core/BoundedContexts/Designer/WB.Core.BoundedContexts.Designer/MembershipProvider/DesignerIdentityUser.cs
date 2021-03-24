using System;
using Microsoft.AspNetCore.Identity;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerIdentityUser : IdentityUser<Guid>
    {
        public virtual string? PasswordSalt { get; set; }

        public virtual bool CanImportOnHq { get; set; }

        public virtual DateTime CreatedAtUtc { get; set; }

        public virtual string? PendingEmail { get; set; }
    }

    public class DesignerIdentityRole : IdentityRole<Guid>
    {
    }
}
