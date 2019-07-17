namespace WB.Infrastructure.Native.Monitoring
{
    public class Histogram
    {
        private readonly Prometheus.Histogram histogram;

        public Histogram(string name, string help, double[] buckets, string[] labelNames)
        {
            this.histogram = Prometheus.Metrics.CreateHistogram(name, help, buckets, labelNames);
        }

        public void Observe(double value, params string[] labels)
        {
            this.histogram.Labels(labels).Observe(value);
        }
    }
}