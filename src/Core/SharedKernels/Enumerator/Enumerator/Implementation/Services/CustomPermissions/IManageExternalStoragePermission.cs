using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.CustomPermissions;

public interface IManageExternalStoragePermission
{
    Task<PermissionStatus> CheckStatusAsync();
    Task<PermissionStatus> RequestAsync();
}