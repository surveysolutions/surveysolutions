using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Domain
{
    public interface IRootScopeExecutor
    {
        Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func);
    }
}