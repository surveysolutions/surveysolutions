using Android.Util;

using Java.Lang;
using WB.Core.GenericSubdomains.Logging;
using Exception = System.Exception;

namespace WB.Core.SharedKernel.Utils.Logging
{
    public static class ExceptionExtension
    {
        public static Throwable ToThrowable(this Exception exception)
        {
            return new Throwable(exception.ToString());
        }
    }

    public class AndroidLogger : ILogger
    {
        private const string Tag = "Android.WBCapi";

        public AndroidLogger()
        {
#if DEBUG
            IsDebugEnabled = true;
#else
            IsDebugEnabled = false;
#endif
            IsErrorEnabled = true;
            IsFatalEnabled = true;
            IsInfoEnabled = true;
            IsWarnEnabled = true;
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled)
                Log.Debug(Tag, message.ToString());
        }

        public void Debug(string message, Exception exception = null)
        {
            if (IsDebugEnabled)
                Log.Debug(Tag, exception.ToThrowable(), message.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
                Log.Debug(Tag, format, args);
        }

        public void Info(object message)
        {
            Log.Info(Tag, message.ToString());
        }

        public void Info(string message, Exception exception = null)
        {
            Log.Info(Tag, exception.ToThrowable(), message.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            Log.Info(Tag, format, args);
        }

        public void Warn(object message)
        {
            Log.Warn(Tag, message.ToString());
        }

        public void Warn(string message, Exception exception = null)
        {
            Log.Warn(Tag, exception.ToThrowable(), message.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            Log.Warn(Tag, format, args);
        }

        public void Error(object message)
        {
            Log.Error(Tag, message.ToString());
        }

        public void Error(string message, Exception exception = null)
        {
            Log.Error(Tag, exception.ToThrowable(), message.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Log.Error(Tag, format, args);
        }

        public void Fatal(object message)
        {
            Log.Wtf(Tag, message.ToString());
        }

        public void Fatal(string message, Exception exception = null)
        {
            Log.Wtf(Tag, exception.ToThrowable(), message.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            Log.Wtf(Tag, format, args);
        }

        public bool IsDebugEnabled { get; private set; }
        public bool IsInfoEnabled { get; private set; }
        public bool IsWarnEnabled { get; private set; }
        public bool IsErrorEnabled { get; private set; }
        public bool IsFatalEnabled { get; private set; }
    }
}