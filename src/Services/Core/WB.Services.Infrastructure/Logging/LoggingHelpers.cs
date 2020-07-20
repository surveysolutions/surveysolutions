using System;
using System.Linq;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Serilog.Events;

namespace WB.Services.Infrastructure.Logging
{
    public static class LoggingHelpers
    {
        public static IDisposable LogContext(string key, object value)
        {
            return LogContext((key, value));
        }

        public static IDisposable LogContext(params (string key, object? value)[] properties)
        {
            var enrichers = properties.Select(p => (ILogEventEnricher)new PropertyEnricher(p.key, p.value)).ToArray();
            return Serilog.Context.LogContext.Push(enrichers);
        }

        public static LoggerConfiguration Postgres(this LoggerSinkConfiguration sinkConfiguration,
            string connectionString, LogEventLevel minLevel = LogEventLevel.Error, string schema = "logs", string tableName = "errors")
        {
            return sinkConfiguration.Sink(new Postgres(connectionString, schema, tableName, minLevel));
        }

        public static LoggerConfiguration Slack(this LoggerSinkConfiguration sinkConfiguration, string webHook, LogEventLevel level, string workerId)
        {
            return sinkConfiguration.Sink(new SlackSink(webHook, level, workerId));
        }
    }
}
