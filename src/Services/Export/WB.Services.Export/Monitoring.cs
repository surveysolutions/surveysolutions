using Prometheus;

namespace WB.Services.Export
{
    public static class Monitoring
    {
        public static class Http
        {
            public const string HTTP_IN = "in";
            public const string HTTP_OUT = "out";

            public static Counter RequestsCounter = Prometheus.Metrics.CreateCounter(
                "wb_http_total",
                "Overall counter for handled requests", "direction", "tenant");

            public static Counter DataServed = Prometheus.Metrics.CreateCounter(
                "wb_http_bytes_count",
                "Overall counter for handled requests bytes", "direction", "tenant");

            public static Histogram Latency = Metrics.CreateHistogram(
                "wb_http_latency",
                "Histogram for requests latency", new[] {0, 0.2, 0.4, 0.6, 0.8, 0.9}
                , "direction", "tenant");
        }
    }
}
