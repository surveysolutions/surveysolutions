using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus;
using WB.Core.BoundedContexts.Headquarters;
using WB.UI.Headquarters.Configs;

namespace WB.UI.Headquarters.Metrics
{
    public class PushGatewayMetricsPusher : IHostedService
    {
        private readonly IOptions<MetricsConfig> options;
        private readonly IOptions<HeadquartersConfig> hqOptions;
        private IMetricServer pusher;

        public PushGatewayMetricsPusher(IOptions<MetricsConfig> options, IOptions<HeadquartersConfig> hqOptions)
        {
            this.options = options;
            this.hqOptions = hqOptions;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!this.options.Value.UsePushGateway) return Task.CompletedTask;

            this.pusher = new MetricPusher(new MetricPusherOptions
            {
                Endpoint = this.options.Value.PushGatewayEndpoint,
                AdditionalLabels = new[] { Tuple.Create("site", hqOptions.Value.TenantName) },
                Job = "hq"
            }).Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if(pusher != null)
            {
                return pusher.StopAsync();
            }

            return Task.CompletedTask;
        }
    }
}
