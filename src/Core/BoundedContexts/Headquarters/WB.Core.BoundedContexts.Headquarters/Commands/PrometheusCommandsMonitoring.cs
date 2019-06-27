using System;
using WB.Core.Infrastructure.CommandBus;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Core.BoundedContexts.Headquarters.Commands
{
    internal class PrometheusCommandsMonitoring : ICommandsMonitoring
    {
        private readonly Histogram histogram;

        public PrometheusCommandsMonitoring()
        {
            this.histogram = new Histogram("wb_commands_time_seconds",
                "measures execution time of command",
                new[] { 0.15, 0.3, 0.5, 1.5, 3 }, 
                new[]{"duration_seconds"});
        }

        public void Report(string commandName, TimeSpan duration)
        {
            this.histogram.Observe(duration.TotalSeconds, commandName);
        }
    }
}
