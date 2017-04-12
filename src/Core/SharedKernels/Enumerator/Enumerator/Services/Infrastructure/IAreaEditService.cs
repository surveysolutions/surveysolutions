using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IAreaEditService
    {
        Task<AreaEditResult> EditAreaAsync();
    }
}
