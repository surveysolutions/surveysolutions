using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("WB.Tests.Unit")]
[assembly: InternalsVisibleTo("WB.Tests.Integration")]
[assembly: InternalsVisibleTo("PerformanceTest")]
[assembly: InternalsVisibleTo("WB.Tests.Abc")]

namespace WB.Core.BoundedContexts.Interviewer
{
    internal static class LinqUtils
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (var item in enumeration)
            {
                action(item);
            }
        }
    }
}