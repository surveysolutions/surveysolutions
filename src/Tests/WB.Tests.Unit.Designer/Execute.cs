using System;
using AppDomainToolkit;

namespace WB.Tests.Unit.Designer
{
    internal static class Execute
    {
        internal static TResult InStandaloneAppDomain<TResult>(AppDomain domain, Func<TResult> function)
        {
#if NCRUNCH
            // ncrunch already creates a standalone app domain for each test
            return function.Invoke();
#else
            return RemoteFunc.Invoke(domain, function);
#endif
        }
    }
}