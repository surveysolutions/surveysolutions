using Microsoft.Practices.ServiceLocation;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public class ServiceLocatorLoggerProvider : ILoggerProvider
    {
        public ILogger GetFor<T>() => ServiceLocator.Current.GetInstance<ILogger>();
    }
}