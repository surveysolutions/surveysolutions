using System;
using System.ComponentModel;
using NLog;
using StackExchange.Exceptional;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.WebTester.Services
{
    public class MetricsService
    {
        public static bool IsEnabled => System.Configuration.ConfigurationManager.AppSettings.GetBool(@"Metrics.Enable", true);

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
                var metricsGateway = System.Configuration.ConfigurationManager.AppSettings["Metrics.Gateway"];
                var instanceName = System.Configuration.ConfigurationManager.AppSettings["InstanceName"] ?? "webtester";

                if (string.IsNullOrEmpty(metricsGateway))
                    return;

                // initialize push mechanizm
                new Prometheus.MetricPusher(metricsGateway, job: "hq",
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