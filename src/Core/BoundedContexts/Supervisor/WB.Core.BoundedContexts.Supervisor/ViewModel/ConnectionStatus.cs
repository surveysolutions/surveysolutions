namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public enum ConnectionStatus
    {
        WaitingForGoogleApi,
        StartDiscovering,
        StartAdvertising,
        Disconnected,
        Connecting,
        Sync,
        Done,
        Connected,
        Advertising,
        Error
    }
}