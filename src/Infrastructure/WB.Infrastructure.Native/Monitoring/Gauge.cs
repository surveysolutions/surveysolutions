using System.Collections.Generic;

namespace WB.Infrastructure.Native.Monitoring
{
    public class Gauge : Gauge.IGauge
    {
        private readonly Prometheus.Gauge gauge;
        private string[] labels;

        public Gauge(string name, string help, params string[] labelNames)
        {
            this.gauge = Prometheus.Metrics.CreateGauge(name, help, labelNames);
        }

        public IEnumerable<string[]> AllLabels => this.gauge.GetAllLabelValues();

        public double Value => this.labels == null ? this.gauge.Value : this.gauge.Labels(labels).Value;

        public void Inc(double amount = 1)
        {
            if (labels == null)
            {
                gauge.Inc(amount);
            }
            else
            {
                gauge.Labels(labels).Inc(amount);
            }
        }

        public void Dec(double amount = 1)
        {
            if (labels == null)
            {
                gauge.Dec(amount);
            }
            else
            {
                gauge.Labels(labels).Dec(amount);
            }
        }

        public void Set(double amount)
        {
            if (labels == null)
            {
                gauge.Set(amount);
            }
            else
            {
                gauge.Labels(labels).Set(amount);
            }
        }

        public IGauge Labels(params string[] labels)
        {
            this.labels = labels;
            return this;
        }

        public interface IGauge
        {
            void Inc(double amount = 1);
            void Dec(double amount = 1);
            void Set(double amount);
            double Value { get; }
        }
    }
}
