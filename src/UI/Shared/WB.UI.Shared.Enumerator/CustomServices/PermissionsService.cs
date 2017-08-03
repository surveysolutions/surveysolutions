using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class PermissionsService : IPermissionsService
    {
        private readonly IPermissions permissions;

        public PermissionsService(IPermissions permissions)
        {
            this.permissions = permissions;
        }

        public Task AssureHasPermission(Permission permission) => this.permissions.AssureHasPermission(permission);
    }
}