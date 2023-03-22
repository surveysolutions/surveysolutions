using System;
using System.Security.Claims;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Code
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? UserId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(claim, out Guid result))
            {
                return null;
            };
            return result;
        }

        public static string UserName(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Name);
        }

        public static bool IsInRole(this ClaimsPrincipal principal, UserRoles role)
        {
            return principal.IsInRole(role.ToString());
        }
    }
}
