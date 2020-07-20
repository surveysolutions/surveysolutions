using System;
using Prometheus;

namespace WB.Services.Export
{
    public static class Monitoring
    {
        public static class Http
        {
            private static readonly Lazy<Counter> RequestCounter = new Lazy<Counter>(() => Metrics.CreateCounter(
                "wb_export_http_total",
                "Overall counter for handled requests", "tenant"));

            private static readonly Lazy<Counter> DataServed = new Lazy<Counter>(() => Metrics.CreateCounter(
                "wb_export_http_bytes_count",
                "Overall counter for handled requests bytes", "tenant"));


            public static void RegisterRequest(string tenant, double duration, long bytes)
            {
                RequestCounter.Value.Labels(tenant).Inc();
                DataServed.Value.Labels(tenant).Inc(bytes);
            }
        }

        private static readonly Counter EventsProcessedCounter = Metrics.CreateCounter(
            "wb_export_events_processed_count",
            "Count of events processed by Export Service", "tenant");


        private static readonly Histogram EventsProcessedCounterLatency = Metrics.CreateHistogram(
                "wb_export_events_processed_latency",
            "Events handling latency by Export Service", "tenant");

        public static void TrackEventsProcessedCount(string? tenant, double value)
        {
            if (tenant != null)
                EventsProcessedCounter.Labels(tenant).Inc(value);
        }

        public static void TrackEventsProcessingLatency(string tenant, long eventsCount, TimeSpan span)
        {
            EventsProcessedCounterLatency.Labels(tenant).Observe(span.TotalSeconds, eventsCount);
        }
    }
}
