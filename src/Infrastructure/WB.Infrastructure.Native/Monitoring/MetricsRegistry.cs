using System.IO;
using System.Threading.Tasks;
using Prometheus;

namespace WB.Infrastructure.Native.Monitoring
{
    public class MetricsRegistry
    {
        public static MetricsRegistry Instance { get; set; } = new MetricsRegistry();

        public void RegisterOnDemandCollectors(params IOnDemandCollector[] collectors)
        {
            foreach (var collector in collectors)
            {
                collector.RegisterMetrics();
            }

            Metrics.DefaultRegistry.AddBeforeCollectCallback(() =>
            {
                foreach (var collector in collectors)
                {
                    collector.UpdateMetrics();
                }
            });
        }

        public static Task Update()
        {
            return Metrics.DefaultRegistry.CollectAndExportAsTextAsync(Stream.Null);
        }
    }
}
