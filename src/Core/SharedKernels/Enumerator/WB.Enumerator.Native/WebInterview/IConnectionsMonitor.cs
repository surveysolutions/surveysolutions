namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IConnectionsMonitor
    {
        void Connected(string connectionId);
        void Disconnected(string connectionId);
        void StartMonitoring();
    }
}