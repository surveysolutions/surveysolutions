using WB.Infrastructure.Native.Monitoring;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    internal class ConnectionLimiter: IConnectionLimiter
    {
        public ConnectionLimiter(int connectionsLimit)
        {
            this.ConnectionLimit = connectionsLimit;
        }

        private int CurrentlyConnectedCount => (int) CommonMetrics.WebInterviewOpenConnections.Value;

        private int ConnectionLimit { get; }

        public bool CanConnect()
        {
            return this.CurrentlyConnectedCount < this.ConnectionLimit;
        }
    }
}