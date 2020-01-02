using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Domain
{
    public interface IInScopeExecutor
    {
        void Execute(Action<IServiceLocator> action);
        T Execute<T>(Func<IServiceLocator, T> func);
        Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func);
        Task ExecuteAsync(Func<IServiceLocator, Task> func);
    }
}
