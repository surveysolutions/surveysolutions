using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.Infrastructure.Domain
{
    public interface IInScopeExecutor
    {
        void Execute(Action<IServiceLocator> action, string workspace = null);
        T Execute<T>(Func<IServiceLocator, T> func, string workspace = null);
        Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func, string workspace = null);
        Task ExecuteAsync(Func<IServiceLocator, Task> func, string workspace = null);
    }

    public interface IInScopeExecutor<out TService>
    {
        void Execute(Action<TService> action, string workspace = null);
        TResult Execute<TResult>(Func<TService, TResult> action, string workspace = null);
        Task ExecuteAsync(Func<TService, Task> action, string workspace = null);
        Task<T> ExecuteAsync<T>(Func<TService, Task<T>> action, string workspace = null);
    }

    public interface IInScopeExecutor<out TService1, out TService2>
    {
        void Execute(Action<TService1, TService2> action, string workspace = null);
        TResult Execute<TResult>(Func<TService1, TService2, TResult> action, string workspace = null);
        Task ExecuteAsync(Func<TService1, TService2, Task> action, string workspace = null);
    }
}
