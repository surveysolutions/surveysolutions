using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IPermissionsService
    {
        Task AssureHasPermissionOrThrow<T>() where T: Permissions.BasePermission, new();
        Task EnsureHasPermissionToInstallFromUnknownSourcesAsync();
        Task<PermissionStatus> CheckPermissionStatusAsync<T>() where T: Permissions.BasePermission, new();
        Task AssureHasExternalStoragePermissionOrThrow();
        Task AssureHasBluetoothAdvertisePermissionOrThrow();
        Task AssureHasNearbyWifiDevicesPermissionOrThrow();
    }
}
