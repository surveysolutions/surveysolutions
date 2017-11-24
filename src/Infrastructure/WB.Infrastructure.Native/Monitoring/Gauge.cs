namespace WB.Infrastructure.Native.Monitoring
{
    public class Gauge
    {
        private readonly Prometheus.Gauge gauge;

        public Gauge(string name, string help, params string[] labelNames)
        {
            this.gauge = Prometheus.Metrics.CreateGauge(name, help, labelNames);
        }

        public void Inc(double amount = 1)
        {
            gauge.Inc(amount);
        }

        public void Dec(double amount = 1)
        {
            gauge.Dec(amount);
        }

        public void Set(double amount)
        {
            gauge.Set(amount);
        }

        public IGauge Label(params string[] labels)
        {
            return new ChildGauge(gauge, labels);
        }

        private class ChildGauge : IGauge
        {
            private readonly Prometheus.Gauge gauge;
            private readonly string[] labels;

            public ChildGauge(Prometheus.Gauge gauge, string[] labels)
            {
                this.gauge = gauge;
                this.labels = labels;
            }

            public void Inc(double amount = 1)
            {
                this.gauge.Labels(labels).Inc(amount);
            }

            public void Dec(double amount = 1)
            {
                this.gauge.Labels(labels).Dec(amount);
            }

            public void Set(double amount)
            {
                gauge.Labels(labels).Set(amount);
            }
        }

        public interface IGauge
        {
            void Inc(double amount = 1);
            void Dec(double amount = 1);
            void Set(double amount);
        }

    }
}