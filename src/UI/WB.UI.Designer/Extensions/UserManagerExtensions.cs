using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Designer.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<string?> GetFullName(this UserManager<DesignerIdentityUser> users, Guid userId)
        {
            var user = await users.FindByIdAsync(userId.FormatGuid());
            if (user != null)
            {
                var claims = await users.GetClaimsAsync(user);
                return claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            }

            return null;
        }
    }
}
