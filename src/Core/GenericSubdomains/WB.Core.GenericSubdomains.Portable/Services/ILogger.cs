using System;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ILogger
    {
        void Debug(string message, Exception exception = null);

        void Error(string message, Exception exception = null);

        void Fatal(string message, Exception exception = null);

        void Info(string message, Exception exception = null);

        void Warn(string message, Exception exception = null);
    }
}