using System.Linq;

namespace WB.Infrastructure.Native.Monitoring
{
    public class MetricsRegistry
    {
        public static MetricsRegistry Instance { get; set; }

        public void RegisterOnDemandCollectors(params IOnDemandCollector[] collectors)
        {
            Prometheus.Advanced.DefaultCollectorRegistry.Instance.RegisterOnDemandCollectors(
                collectors.Select(Convert).ToArray());
        }

        private Prometheus.Advanced.IOnDemandCollector Convert(IOnDemandCollector onDemandCollector)
        {
            return new OnDemandCollectorWrappe(onDemandCollector);
        }

        private class OnDemandCollectorWrappe : Prometheus.Advanced.IOnDemandCollector
        {
            private readonly IOnDemandCollector collector;

            public OnDemandCollectorWrappe(IOnDemandCollector collector)
            {
                this.collector = collector;
            }

            public void RegisterMetrics()
            {
                collector.RegisterMetrics();
            }

            public void UpdateMetrics()
            {
                collector.UpdateMetrics();
            }
        }
    }
}