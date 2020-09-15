using System.Threading.Tasks;
using Android.OS;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.Services
{
    public static class PermissionsExtensions
    {
        public static async Task AssureHasPermissionOrThrow<T>(this IPermissions permissions) where T : BasePermission, new()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M) return;
            if (await permissions.CheckPermissionStatusAsync<T>().ConfigureAwait(false) == PermissionStatus.Granted) return;

            var permissionsRequest = await permissions.RequestPermissionAsync<T>().ConfigureAwait(false);
            if (permissionsRequest != PermissionStatus.Granted)
                throw new MissingPermissionsException(UIResources.MissingPermission, typeof(T));
        }
    }
}
