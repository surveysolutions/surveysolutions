using System.Security.Principal;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Shared.Web.Extensions
{
    public static class IPrincipalExtensions 
    {
        public static bool HasAnyOfRoles(this IPrincipal principal, params UserRoles[] any)
        {
            foreach (var role in any)
            {
                if (principal.IsInRole(role.ToString()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}