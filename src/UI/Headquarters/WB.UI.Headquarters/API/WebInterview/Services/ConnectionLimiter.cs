using System.Linq;
using System.Web.WebPages;
using Prometheus.Advanced;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    internal class ConnectionLimiter: IConnectionLimiter
    {
        private readonly IConfigurationManager configurationManager;

        public ConnectionLimiter(IConfigurationManager configurationManager)
        {
            this.configurationManager = configurationManager;
        }

        private int CurrentlyConnectedCount
        {
            get
            {
                var metric = DefaultCollectorRegistry.Instance.CollectAll()
                    .Where(f => f.name == ConnectedMetricName)
                    .SelectMany(f => f.metric).FirstOrDefault();

                return (int) (metric?.gauge?.value ?? 0d);
            }
        }

        private int ConnectionLimit => this.configurationManager.AppSettings[@"MaxWebInterviewsCount"].AsInt();

        public bool CanConnect()
        {
            return this.CurrentlyConnectedCount < this.ConnectionLimit;
        }

        public const string ConnectedMetricName = "webinterview_connected_count";
    }
}