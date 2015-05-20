using WB.Core.GenericSubdomains.Portable.Services;

namespace Ncqrs
{
    /// <summary>
    /// These extentions are only for NCQRS usage. So NCQRS now uses our logger without a lot of changes.
    /// DO NOT MAKE IT PUBLIC!
    /// </summary>
    internal static class LoggerExtensions
    {
        internal static void DebugFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Debug(string.Format(format, args));
        }

        internal static void WarnFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Warn(string.Format(format, args));
        }

        internal static void InfoFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Info(string.Format(format, args));
        }

        internal static void FatalFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Fatal(string.Format(format, args));
        }
    }
}
