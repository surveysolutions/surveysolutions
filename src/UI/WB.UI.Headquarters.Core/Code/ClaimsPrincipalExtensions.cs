using System;
using System.Security.Claims;

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
    }
}
