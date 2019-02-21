using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;
using Microsoft.Extensions.Logging;
using Prometheus;
using WB.Services.Export.InterviewDataStorage;

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

        private static readonly Gauge EventsProcessedCounter = Metrics.CreateGauge("wb_services_export_events_processed_count",
            "Count of events processed by Export Service", "site");

        public static void TrackEventsProcessedCount(string tenant, double value)
        {
            if (tenant != null)
                EventsProcessedCounter.Labels(tenant).Set(value);
        }

        private static readonly Gauge HandlerEventHandlingSpeedGauge = Metrics.CreateGauge("wb_services_export_events_handler_speed",
            "Events handling speed of each event handler", "site", "handler");

        private static readonly Gauge SqlCommandsExecutionGauge = Metrics.CreateGauge("wb_services_export_sql_generated",
            "Count of commands of each type", "site", "type");

        public static void TrackEventHandlerProcessingSpeed(string tenant, Type handler, double value)
        {
            var handlerName = handler?.Name.Humanize(LetterCasing.LowerCase).Replace(" ", "_");

            if (tenant != null && handler != null)
                HandlerEventHandlingSpeedGauge.Labels(tenant, handlerName).Set(value);
        }

        public static void TrackEventHandlerProcessingSpeed(string tenant, string name, double value)
        {
            if (tenant != null && name != null)
                HandlerEventHandlingSpeedGauge.Labels(tenant, name).Set(value);
        }

        public static void ResetEventHandlerProcessingSpeed(string tenant)
        {
            foreach (var family in HandlerEventHandlingSpeedGauge.Collect())
            {
                foreach (var metric in family.metric)
                {
                    if(metric.label[0].value != tenant) continue;

                    HandlerEventHandlingSpeedGauge.Labels(metric.label.Select(l => l.value).ToArray()).Set(0);
                }
            }
        }

        public static void TrackSqlCommandsGeneration(string tenant, string sqlCommandType, double count)
        {
            if (tenant == null) return;
            SqlCommandsExecutionGauge.Labels(tenant, sqlCommandType).Set(count);
        }

        public static void DumpSqlCommandsTrack(ILogger<InterviewDataDenormalizer> logger, LogLevel level)
        {

            var sb = new StringBuilder();

            sb.Append("Executed commands: ");
            var args = new List<(object, int)>();

            foreach (var family in SqlCommandsExecutionGauge.Collect())
            {
                foreach (var metric in family.metric)
                {
                    sb.Append($" {{{metric.label[1].value}}}={{{metric.label[1].value}_val}}");
                    args.Add((metric.label[1].value, (int)metric.gauge.value));
                }
            }
            logger.Log(level, sb.ToString(), args.SelectMany(a => new [] {a.Item1, a.Item2}).ToArray());
            //    var sb = new Microsoft.Extensions.ObjectPool.DefaultObjectPoolProvider();
            //    var pool = sb.Create(new StringBuilderPooledObjectPolicy());

            //    pool.Get()

            //    SqlCommandsExecutionGauge.Collect().
        }
    }
}
