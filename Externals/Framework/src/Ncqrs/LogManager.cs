using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
