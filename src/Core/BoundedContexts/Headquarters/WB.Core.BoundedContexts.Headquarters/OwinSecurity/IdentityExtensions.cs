using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public static class IdentityExtensions
    {
        public static bool IsInRole(this HqUser user, UserRoles role)
        {
            return user.Roles.Any(r => r.Role == role);
        }

        public static IdentityResult ChangePassword(this HqUserManager hqUserManager, HqUser user, string password)
        {
            return AsyncHelper.RunSync(() => hqUserManager.ChangePasswordAsync(user, password));
        }
    }
}