namespace WB.Infrastructure.Native.Monitoring
{
    public class Counter : Counter.ICounter
    {
        private readonly Prometheus.Counter counter;

        public Counter(string name, string help, params string[] labelNames)
        {
            this.counter = Prometheus.Metrics.CreateCounter(name, help, labelNames);
        }

        public void Inc(double amount = 1)
        {
            counter.Inc(amount);
        }

        public ICounter Label(params string[] labels)
        {
            return new ChildCounter(counter, labels);
        }

        private class ChildCounter : ICounter
        {
            private readonly Prometheus.Counter counter;
            private readonly string[] labels;

            public ChildCounter(Prometheus.Counter counter, string[] labels)
            {
                this.counter = counter;
                this.labels = labels;
            }

            public void Inc(double amount = 1)
            {
                this.counter.Labels(labels).Inc(amount);
            }
        }

        public interface ICounter
        {
            void Inc(double amount = 1);
        }
    }
}