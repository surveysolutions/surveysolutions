using System;
using Microsoft.AspNetCore.Identity;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerIdentityUser : IdentityUser
    {
        public string PasswordSalt { get; set; }

        public bool CanImportOnHq { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
