using System;
using System.Runtime.CompilerServices;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class LoggingExtensions
    {
        public static void Verbose(this ILogger logger, string message = null, [CallerMemberName] string method = null)
        {
            message = message == null ? "" : ": " + message;
            logger.Trace(method + message);
        }

        public static void ErrorVerbose(this ILogger logger, string message, Exception e, [CallerMemberName] string method = null)
        {
            message = message == null ? "" : ": " + message;
            logger.Error(method + message, e);
        }
    }
}
