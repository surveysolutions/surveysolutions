using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Logging
{
    internal class NLogLoggerProvider : ILoggerProvider
    {
        public ILogger GetFor<T>() => new NLogLogger(typeof(T));
    }
}