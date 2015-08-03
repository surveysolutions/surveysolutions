using System;

namespace WB.Core.BoundedContexts.Tester.Infrastructure
{
    public interface ILogger
    {
        void Error(string message, Exception exception = null);
        void Warn(string message, Exception exception = null);
    }
}