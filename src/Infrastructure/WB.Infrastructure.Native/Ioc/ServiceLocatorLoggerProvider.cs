using System;
using Microsoft.Practices.ServiceLocation;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public class ServiceLocatorLoggerProvider : ILoggerProvider
    {
        public ILogger GetFor<T>() => ServiceLocator.Current.GetInstance<ILogger>();

        public ILogger GetForType(Type type) => ServiceLocator.Current.GetInstance<ILogger>();
    }
}