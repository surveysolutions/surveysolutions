using Android;
using Xamarin.Essentials;

namespace WB.UI.Shared.Enumerator.PermissionsCustom;


public class NearbyWifiDevicesPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new (string, bool)[] { 
            (Manifest.Permission.NearbyWifiDevices, true),
        };
}