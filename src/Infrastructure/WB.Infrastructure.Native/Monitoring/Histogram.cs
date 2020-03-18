using Prometheus;

namespace WB.Infrastructure.Native.Monitoring
{
    public class Histogram
    {
        private readonly Prometheus.Histogram histogram;

        public Histogram(string name, string help, double[] buckets, string[] labelNames)
        {
            this.histogram = Prometheus.Metrics.CreateHistogram(name, help, new HistogramConfiguration
            {
                Buckets = buckets,
                LabelNames = labelNames
            });
        }

        public void Observe(double value)
        {
            this.histogram.Observe(value);
        }

        public void Observe(double value, params string[] labels)
        {
            this.histogram.Labels(labels).Observe(value);
        }
    }
}
