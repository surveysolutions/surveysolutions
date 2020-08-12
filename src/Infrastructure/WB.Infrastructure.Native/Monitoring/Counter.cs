using System.Collections.Generic;
using WB.Core.Infrastructure.Metrics;

namespace WB.Infrastructure.Native.Monitoring
{
    public class Counter : ICounter
    {
        private readonly Prometheus.Counter counter;
        private string[] labels = null;

        public Counter(string name, string help, params string[] labelNames)
        {
            this.counter = Prometheus.Metrics.CreateCounter(name, help, labelNames);
        }

        public void Inc(double amount = 1)
        {
            if (labels != null)
            {
                counter.Labels(labels).Inc(amount);
            }
            else
            {
                counter.Inc(amount);
            }
        }

        public ICounter Labels(params string[] labels)
        {
            this.labels = labels;
            return this;
        }

        public IEnumerable<string[]> AllLabels => this.counter.GetAllLabelValues();
        public double Value => this.labels == null ? this.counter.Value : this.counter.Labels(labels).Value;
    }
}
