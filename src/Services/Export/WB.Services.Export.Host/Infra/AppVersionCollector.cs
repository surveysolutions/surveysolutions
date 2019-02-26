using Prometheus;
using Prometheus.Advanced;
using WB.Services.Export.Services;

namespace WB.Services.Export.Host.Infra
{
    public class AppVersionCollector : IOnDemandCollector
    {
        private readonly IProductVersion productVersion;

        public AppVersionCollector(IProductVersion productVersion)
        {
            this.productVersion = productVersion;
        }

        public void RegisterMetrics(ICollectorRegistry registry)
        {
            
        }

        private readonly Gauge version = Metrics.CreateGauge("wb_services_export_version",
            "Version of running export",
            "version");

        public void UpdateMetrics()
        {
            version.Labels(this.productVersion.ToString()).Set(1);
        }
    }
}
