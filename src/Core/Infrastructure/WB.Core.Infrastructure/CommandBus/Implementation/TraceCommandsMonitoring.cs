using System;

namespace WB.Core.Infrastructure.CommandBus.Implementation
{
    public class TraceCommandsMonitoring : ICommandsMonitoring
    {
        public void Report(string commandName, TimeSpan duration)
        {
            System.Diagnostics.Trace.Write($"{commandName} took {duration.TotalMilliseconds:N}ms");
        }
    }
}
