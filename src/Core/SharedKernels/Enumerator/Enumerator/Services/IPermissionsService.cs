using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IPermissionsService
    {
        Task AssureHasPermissionOrThrow<T>() where T: BasePermission, new();
        Task EnsureHasPermissionToInstallFromUnknownSourcesAsync();
    }
}
