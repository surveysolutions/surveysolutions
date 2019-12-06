using System;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;

namespace WB.Core.Infrastructure
{
    public static class Extensions
    {
        public static void ExecuteInScope<T>(this IServiceLocator sl, Action<T> action)
        {
            sl.GetInstance<IInScopeExecutor>().Execute(scope =>
            {
                action(scope.GetInstance<T>());
            });
        }
    }
}
