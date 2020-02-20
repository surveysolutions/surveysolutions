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
                case "":
                    CommonMetrics.NpgsqlConnectionsPoolCount.Set(Convert.ToSingle(eventPayload["Count"]));
                    break;
                case "bytes-written-per-second":
                    CommonMetrics.NpgsqlDataCounter.Labels("write").Inc(Convert.ToSingle(eventPayload["Increment"]));
                    break;
                case "bytes-read-per-second":
                    CommonMetrics.NpgsqlDataCounter.Labels("read").Inc(Convert.ToSingle(eventPayload["Increment"]));
                    break;
            }
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name.Equals("Npgsql", StringComparison.OrdinalIgnoreCase))
            {
                EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All, new Dictionary<string, string>
                {
                    {"EventCounterIntervalSec", "1"}
                });
            }
        }
    }
}
