using Android;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.CustomPermissions;
using Xamarin.Essentials;

namespace WB.UI.Shared.Enumerator.CustomServices.CustomPermissions
{
    public class ManageExternalStoragePermission : Permissions.BasePlatformPermission, IManageExternalStoragePermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new (string, bool)[]
            {
                (Manifest.Permission.ManageExternalStorage, true),
                //(Manifest.Permission.WriteExternalStorage, true),
                //(Manifest.Permission.ReadExternalStorage, true)
            };
    }
}