using System;

namespace WB.Tests.Integration
{
    internal static class Execute
    {
        internal static TResult InStandaloneAppDomain<TResult>(AppDomain domain, Func<TResult> function)
        {
            return function.Invoke();
        }
    }
}