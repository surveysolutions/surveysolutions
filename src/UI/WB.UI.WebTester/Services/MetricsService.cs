using System;
using System.ComponentModel;
using System.Web.WebPages;
using NLog;
using StackExchange.Exceptional;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.WebTester.Services
{
    public class MetricsService
    {
        public static bool IsEnabled => ConfigurationSource.Configuration[@"Metrics.Enable"].AsBool(true);

        [Localizable(false)]
        public static void Start(Logger logger)
        {
            if (!IsEnabled)
            {
                logger.Info("Metrics are disabled per configuration");
                return;
            }
            try
            {
                // configuring address for metrics pushgateway
                var metricsGateway = ConfigurationSource.Configuration["Metrics.Gateway"];
                var instanceName = ConfigurationSource.Configuration["InstanceName"] ?? "webtester";

                if (string.IsNullOrEmpty(metricsGateway))
                    return;

                // initialize push mechanizm
                new Prometheus.MetricPusher(metricsGateway, job: "webtester",
                    additionalLabels: new[] { Tuple.Create("site", instanceName) },
                    intervalMilliseconds: 5000).Start();
            }
            catch (Exception e)
            {
                e.Log(null);
                logger.Error("Unable to start metrics push", e);
            }
        }
    }
}