using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public interface IUserManager
    {
        Task<IList<DesignerIdentityUser>> GetUsersInRoleAsync(SimpleRoleEnum role);
    }

    class DesignerUserManager : IUserManager
    {
        private readonly UserManager<DesignerIdentityUser> userManager;

        public DesignerUserManager(UserManager<DesignerIdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        public Task<IList<DesignerIdentityUser>> GetUsersInRoleAsync(SimpleRoleEnum role)
        {
            return userManager.GetUsersInRoleAsync(role.ToString());
        }
    }
}
