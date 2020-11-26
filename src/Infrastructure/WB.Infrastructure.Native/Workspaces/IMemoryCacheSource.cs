using Microsoft.Extensions.Caching.Memory;

namespace WB.Infrastructure.Native.Workspaces
{
    public interface IMemoryCacheSource
    {
        IMemoryCache GetCache(string workspace);
    }
}
