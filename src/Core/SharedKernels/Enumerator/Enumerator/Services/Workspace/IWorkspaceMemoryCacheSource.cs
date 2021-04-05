using Microsoft.Extensions.Caching.Memory;

namespace WB.Core.SharedKernels.Enumerator.Services.Workspace
{
    public interface IWorkspaceMemoryCacheSource
    {
        IMemoryCache GetCache(string workspace);
    }
}