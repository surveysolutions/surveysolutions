using System;
using System.Security.Claims;
using System.Security.Principal;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;

namespace WB.UI.Designer.Extensions
{
    public static class IdentityExtensions
    {
        public static Guid GetId(this ClaimsPrincipal identity)
        {
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            return Guid.Parse(userId);
        }

        public static string GetUserName(this ClaimsPrincipal identity)
        {
            var userName = identity.FindFirst(ClaimTypes.Name).Value;
            return userName;
        }

        public static bool IsAdmin(this IPrincipal identity)
        {
            return identity.IsInRole(SimpleRoleEnum.Administrator.ToString());
        }

        public static Guid GetId(this IPrincipal identity)
        {
            return ((ClaimsPrincipal) identity).GetId();
        }
    }

}
