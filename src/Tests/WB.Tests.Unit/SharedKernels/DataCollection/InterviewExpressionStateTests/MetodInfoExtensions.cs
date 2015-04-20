using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewExpressionStateTests
{
    internal static class MetodInfoExtensions
    {
        public static MetodInfo Get(this IEnumerable<MetodInfo> methodInfos, string methodName)
        {
            return methodInfos.First(m => m.Name == methodName);
        }

        public static MetodInfo GetByMethodNameAndReturnType(this IEnumerable<MetodInfo> methodInfos, string methodName, Type returnType)
        {
            return methodInfos.First(m => m.Name == methodName && m.ReturnType == returnType);
        }
    }
}