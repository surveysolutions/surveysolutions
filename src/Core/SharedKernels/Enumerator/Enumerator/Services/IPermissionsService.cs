using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IPermissionsService
    {
        Task AssureHasPermission(Permission permission);
    }
}