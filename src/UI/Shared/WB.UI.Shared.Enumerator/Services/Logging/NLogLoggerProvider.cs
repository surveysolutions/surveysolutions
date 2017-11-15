using System;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Enumerator.Services.Logging
{
    public class NLogLoggerProvider : ILoggerProvider
    {
        public ILogger GetFor<T>() => new NLogLogger(typeof(T));

        public ILogger GetForType(Type type) => new NLogLogger(type);
    }
}