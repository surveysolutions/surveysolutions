using Java.Lang;
using Exception = System.Exception;

namespace WB.Core.GenericSubdomains.Logging.AndroidLogger
{
    internal static class ExceptionExtension
    {
        public static Throwable ToThrowable(this Exception exception)
        {
            return new Throwable(exception.ToString());
        }
    }
}