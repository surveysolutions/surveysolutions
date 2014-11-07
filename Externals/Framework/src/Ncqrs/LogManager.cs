using System;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;

namespace Ncqrs
{
    internal static class LogManager
    {
        public static ILogger GetLogger(Type type)
        {
            return ServiceLocator.Current.GetInstance<ILogger>();
        }
    }
}
