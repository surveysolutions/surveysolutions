using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Infrastructure.Shared.Enumerator
{
    public static class PermissionsExtensions
    {
        public static async Task AssureHasPermission(this IPermissions permissions, Permission permission)
        {
            if (await permissions.CheckPermissionStatusAsync(Permission.Storage).ConfigureAwait(false) != PermissionStatus.Granted)
            {
                var permissionsRequest = await permissions.RequestPermissionsAsync(Permission.Storage);
                if (permissionsRequest[Permission.Storage] != PermissionStatus.Granted)
                {
                    throw new MissingPermissionsException(UIResources.MissingPermission);
                }
            }
        }
    }
}