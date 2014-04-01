using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace WB.UI.Headquarters.Extensions
{
    public static class IdentityExtensions
    {
        public static Guid UserId(this IPrincipal principal)
        {
            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                throw new InvalidCastException(string.Format("principal should be of type {0} to get his Id with this exension method", typeof(ClaimsPrincipal)));
            }

            Claim sidClaim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid);

            if (sidClaim == null)
            {
                throw new InvalidOperationException(string.Format("Principal should have claim of type {0} to support Id", ClaimTypes.Sid));
            }

            return Guid.Parse(sidClaim.Value);
        }
    }
}