using System;
using System.Security.Claims;
using System.Security.Principal;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer
{
    public static class IdentityHelpers
    {
        public static bool IsAdmin(this IPrincipal identity)
        {
            return identity.IsInRole(SimpleRoleEnum.Administrator.ToString());
        }
        
        public static Guid GetId(this IPrincipal identity)
        {
            return ((ClaimsPrincipal)identity).GetId();
        }

        public static Guid GetId(this ClaimsPrincipal identity)
        {
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) throw new InvalidOperationException($"Claim {ClaimTypes.NameIdentifier} was not found");
            return Guid.Parse(userId);
        }

        public static string GetUserName(this ClaimsPrincipal identity)
        {
            var userName = identity.FindFirst(ClaimTypes.Name);
            if (userName == null) throw new InvalidOperationException($"Claim {ClaimTypes.Name} was not found");
            
            return userName.Value;
        }
    }
}
