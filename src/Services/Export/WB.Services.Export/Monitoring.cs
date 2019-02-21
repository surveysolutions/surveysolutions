﻿using System;
using Prometheus;

namespace WB.Services.Export
{
    public static class Monitoring
    {
        public static class Http
        {
            private static readonly Lazy<Counter> RequestCounter = new Lazy<Counter>(() => Metrics.CreateCounter(
                "wb_services_export_http_total",
                "Overall counter for handled requests", "tenant"));

            private static readonly Lazy<Counter> DataServed = new Lazy<Counter>(() => Metrics.CreateCounter(
                "wb_services_export_http_bytes_count",
                "Overall counter for handled requests bytes", "tenant"));

            public static readonly Lazy<Histogram> Latency = new Lazy<Histogram>(() => Metrics.CreateHistogram(
                "wb_services_export_http_latency",
                "Histogram for requests latency", new[] {0, 0.2, 0.4, 0.6, 0.8, 0.9}
                , "tenant"));

            public static void RegisterRequest(string tenant, double duration, long bytes)
            {
                RequestCounter.Value.Labels(tenant).Inc();
                DataServed.Value.Labels(tenant).Inc(bytes);
                Latency.Value.Labels(tenant).Observe(duration);
            }
        }

        public static readonly Gauge EventsProcessedCounter = Metrics.CreateGauge("wb_services_export_events_processed_count",
            "Count of events processed by Export Service", "site");

        public static readonly Gauge HandlerEventHandlingSpeedGauge = Metrics.CreateGauge("wb_services_export_events_handler_speed",
            "Events handling speed of each event handler", new [] {"site", "handler"});

        public static void TrackEventHandlerProcessingSped(string tenant, string handlerName, double value)
        {
            if(tenant != null && handlerName != null)
            HandlerEventHandlingSpeedGauge.Labels(tenant, handlerName).Set(value);
        }

        public static void TrackEventsProcessedCount(string tenant, double value)
        {
            if(tenant != null)
                EventsProcessedCounter.Labels(tenant).Set(value);
        }
    }
}
