namespace WB.Core.Synchronization
{
    public interface IIncomingSyncPackagesQueue
    {
        void PushSyncItem(string item);
        int QueueLength { get; }
        string DeQueue();
    }
}
