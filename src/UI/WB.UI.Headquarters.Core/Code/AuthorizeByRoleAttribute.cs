using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;

namespace WB.UI.Headquarters.Code
{
    public class AuthorizeByRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeByRoleAttribute(params UserRoles[] userRoles)
        {
            Roles = string.Join(", ", userRoles.Select(role => role.ToString()));
        }
    }
}
