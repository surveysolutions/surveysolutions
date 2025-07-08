using System;
using Serilog;

namespace WB.UI.Shared.Web.LoggingIntegration
{
    public class SerilogLogger : Core.GenericSubdomains.Portable.Services.ILogger
    {
        private readonly Serilog.ILogger logger;

        public SerilogLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void Trace(string message, Exception? exception = null)
        {
            this.logger.Verbose(exception, SanitizeMessage(message));
        }

        public void Debug(string message, Exception? exception = null)
        {
            this.logger.Debug(exception, SanitizeMessage(message));
        }

        public void Info(string message, Exception? exception = null)
        {
            this.logger.Information(exception, SanitizeMessage(message));
        }

        public void Warn(string message, Exception? exception = null)
        {
            this.logger.Warning(exception, SanitizeMessage(message));
        }

        public void Error(string message, Exception? exception = null)
        {
            this.logger.Error(exception, SanitizeMessage(message));
        }

        public void Fatal(string message, Exception? exception = null)
        {
            this.logger.Fatal(exception, SanitizeMessage(message));
        }

        private string SanitizeMessage(string message)
        {
            // Implement sanitization logic here, e.g., masking sensitive data
            // For demonstration, replace sensitive keywords with "[REDACTED]"
            if (string.IsNullOrEmpty(message)) return message;
            return message.Replace("changePassword", "[REDACTED]");
        }
    }
}
