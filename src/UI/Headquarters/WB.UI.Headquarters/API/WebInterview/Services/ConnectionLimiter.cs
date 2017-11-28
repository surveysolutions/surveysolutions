using System.Web.WebPages;
using WB.Infrastructure.Native.Monitoring;
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

        private int CurrentlyConnectedCount => (int) CommonMetrics.WebInterviewOpenConnections.Value;

        private int ConnectionLimit => this.configurationManager.AppSettings[@"MaxWebInterviewsCount"].AsInt();

        public bool CanConnect()
        {
            return this.CurrentlyConnectedCount < this.ConnectionLimit;
        }
    }
}