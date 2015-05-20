using System;
using WB.Core.GenericSubdomains.Portable.Services;

namespace Ncqrs
{
    internal static class LogManager
    {
        public static ILogger GetLogger(Type type)
        {
            return NcqrsEnvironment.Get<ILogger>();
        }
    }
}
