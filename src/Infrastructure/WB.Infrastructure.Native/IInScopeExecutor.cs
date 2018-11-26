using System;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IInScopeExecutor
    {
        void ExecuteActionInScope(Action<IServiceLocator> action);
        bool ExecuteFunctionInScope(Func<IServiceLocator, bool> func);
    }
}