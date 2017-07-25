using System.Threading.Tasks;
using Android.OS;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.Services
{
    public static class PermissionsExtensions
    {
        public static async Task AssureHasPermission(this IPermissions permissions, Permission permission)
        {
            if ((int)Build.VERSION.SdkInt < 23) return;
            if (await permissions.CheckPermissionStatusAsync(permission).ConfigureAwait(false) == PermissionStatus.Granted) return;

            var permissionsRequest = await permissions.RequestPermissionsAsync(permission).ConfigureAwait(false);
            if (permissionsRequest[permission] != PermissionStatus.Granted)
                throw new MissingPermissionsException(UIResources.MissingPermission, permission);
        }
    }
}