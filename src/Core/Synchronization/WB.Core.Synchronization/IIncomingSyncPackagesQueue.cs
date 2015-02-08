namespace WB.Core.Synchronization
{
    public interface IIncomingSyncPackagesQueue
    {
        void PushSyncItem(string item);
        int QueueLength { get; }
        IncomingSyncPackages DeQueue();
        void DeleteSyncItem(string syncItemPath);
    }
}
