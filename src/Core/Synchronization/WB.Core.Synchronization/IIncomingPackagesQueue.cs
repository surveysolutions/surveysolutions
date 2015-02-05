namespace WB.Core.Synchronization
{
    public interface IIncomingPackagesQueue
    {
        void PushSyncItem(string item);
        int QueueLength { get; }
        string DeQueue();
    }
}
