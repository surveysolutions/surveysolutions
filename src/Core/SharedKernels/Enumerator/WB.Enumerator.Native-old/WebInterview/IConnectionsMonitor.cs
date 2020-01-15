namespace WB.Enumerator.Native.WebInterview
{
    public interface IConnectionsMonitor
    {
        void Connected(string connectionId);
        void Disconnected(string connectionId);
        void StartMonitoring();
    }
}