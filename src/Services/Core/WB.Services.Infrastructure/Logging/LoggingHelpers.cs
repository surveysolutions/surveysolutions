using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;

namespace WB.Services.Infrastructure.Logging
{
    public static class LoggingHelpers
    {
        public static IDisposable LogContext(params (string key, object value)[] properties)
        {
            var enrichers = properties.Select(p => (ILogEventEnricher) new PropertyEnricher(p.key, p.value)).ToArray();
            return Serilog.Context.LogContext.Push(enrichers);
        }
    }
}
