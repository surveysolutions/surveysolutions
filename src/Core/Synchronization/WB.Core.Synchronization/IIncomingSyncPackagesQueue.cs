namespace WB.Core.Synchronization
{
    public interface IIncomingSyncPackagesQueue
    {
        void Enqueue(string item);
        int QueueLength { get; }
        IncomingSyncPackage DeQueue();
        void DeleteSyncItem(string syncItemPath);
    }
}
