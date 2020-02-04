using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WB.Services.Export.Tests.ExportJobTests
{
    public class TestLogger<T> : ILogger<T>
    {
        public class CallLog
        {
            public LogLevel LogLevel { get; set; }
            public EventId EventId { get; set; }
            public object State { get; set; }
            public Exception Exception { get; set; }
        }

        public List<CallLog> CallsLog { get; } = new List<CallLog>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            CallsLog.Add(new CallLog
            {
                LogLevel = logLevel,
                Exception = exception, EventId = eventId, State = state
            });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
