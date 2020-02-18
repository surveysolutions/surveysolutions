using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus;
using WB.UI.Headquarters.Configs;

namespace WB.UI.Headquarters.Metrics
{
    public class PushGatewayMetricsPusher : IHostedService
    {
        private readonly IOptions<MetricsConfig> options;
        private IMetricServer pusher;

        public PushGatewayMetricsPusher(IOptions<MetricsConfig> options)
        {
            this.options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(!this.options.Value.UsePushGateway) return Task.CompletedTask;

            this.pusher = new MetricPusher(new MetricPusherOptions
            {
                Endpoint = this.options.Value.Endpoint,
                Job = "hq"
            }).Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return pusher.StopAsync();
        }
    }
}
