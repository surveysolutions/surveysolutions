using Android.Util;
using NLog;
using NLog.Targets;

namespace WB.UI.Shared.Enumerator.Utils
{
    [Target("LogCat")]
    public sealed class LogCatTarget : TargetWithLayout
    {
        public LogCatTarget()
        {
            this.Tag = "NLog";
        }

        public string Tag { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = this.Layout.Render(logEvent);

            SendTheMessageToLogCat(this.Tag, logMessage, logEvent.Level);
        }

        private void SendTheMessageToLogCat(string tag, string message, LogLevel level)
        {
            switch (level.Name.ToLower())
            {
                case "trace":
                    Log.Verbose(tag, message);
                    break;
                case "debug":
                    Log.Debug(tag, message);
                    break;
                case "info":
                    Log.Info(tag, message);
                    break;
                case "warn":
                    Log.Warn(tag, message);
                    break;
                case "error":
                    Log.Error(tag, message);
                    break;
                case "fatal":
                    Log.Wtf(tag, message);
                    break;
            }
        }
    }
}
