namespace WB.Core.Synchronization
{
    public interface IIncomingSyncPackagesQueue
    {
        void Enqueue(string item);
        int QueueLength { get; }
        IncomingSyncPackages DeQueue();
        void DeleteSyncItem(string syncItemPath);
    }
}
