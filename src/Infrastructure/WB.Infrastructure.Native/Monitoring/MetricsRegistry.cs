using System.Linq;
using Prometheus.Advanced;

namespace WB.Infrastructure.Native.Monitoring
{
    public class MetricsRegistry
    {
        public static MetricsRegistry Instance { get; set; } = new MetricsRegistry();

        public void RegisterOnDemandCollectors(params IOnDemandCollector[] collectors)
        {
            Prometheus.Advanced.DefaultCollectorRegistry.Instance.RegisterOnDemandCollectors(
                collectors.Select(Convert).ToArray());
        }

        private Prometheus.Advanced.IOnDemandCollector Convert(IOnDemandCollector onDemandCollector)
        {
            return new OnDemandCollectorWrapper(onDemandCollector);
        }

        private class OnDemandCollectorWrapper : Prometheus.Advanced.IOnDemandCollector
        {
            private readonly IOnDemandCollector collector;

            public OnDemandCollectorWrapper(IOnDemandCollector collector)
            {
                this.collector = collector;
            }
            
            public void RegisterMetrics(ICollectorRegistry registry)
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
