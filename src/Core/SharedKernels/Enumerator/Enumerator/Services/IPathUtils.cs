using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IPathUtils
    {
        Task<string> GetRootDirectoryAsync();
    }
}
