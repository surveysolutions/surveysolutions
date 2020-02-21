using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WB.Infrastructure.Native.Monitoring;

namespace WB.UI.Headquarters.Metrics
{
    internal sealed class NpgsqlMetricsCollectionService : EventListener, IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventName != "EventCounters"
                || eventData.Payload.Count <= 0
                || !(eventData.Payload[0] is IDictionary<string, object> data)
            )
                return;

            WriteCounters(data);
        }

        private void WriteCounters(IDictionary<string, object> eventPayload)
        {
            switch (eventPayload["Name"])
            {
                case "idle-connections":
                    CommonMetrics.NpgsqlConnections.Labels("idle").Set(Convert.ToSingle(eventPayload["Count"]));
                    break;
                case "busy-connections":
                    CommonMetrics.NpgsqlConnections.Labels("busy").Set(Convert.ToSingle(eventPayload["Count"]));
                    break;
                case "connection-pools":
                    CommonMetrics.NpgsqlConnectionsPoolCount.Set(Convert.ToSingle(eventPayload["Count"]));
                    break;
                case "bytes-written-per-second":
                    var written = Convert.ToSingle(eventPayload["Increment"]);
                    if (written > 0) CommonMetrics.NpgsqlDataCounter.Labels("write").Inc(written);
                    break;
                case "bytes-read-per-second":
                    var read = Convert.ToSingle(eventPayload["Increment"]);
                    if(read > 0 ) CommonMetrics.NpgsqlDataCounter.Labels("read").Inc(read);
                    break;
            }
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name.Equals("Npgsql", StringComparison.OrdinalIgnoreCase))
            {
                EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.None, new Dictionary<string, string>
                {
                    {"EventCounterIntervalSec", "1"}
                });
            }
        }
    }
}
