using Android;
using Xamarin.Essentials;

namespace WB.UI.Shared.Enumerator.PermissionsCustom;

public class BluetoothPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new (string, bool)[] { 
            (Manifest.Permission.BluetoothScan, true),
            (Manifest.Permission.BluetoothConnect, true),
            (Manifest.Permission.BluetoothAdvertise, true),
        };
}