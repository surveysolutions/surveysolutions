namespace Synchronization.Core.SynchronizationFlow
{
    public interface ISynchronizer
    {
        ISynchronizer SetNext(ISynchronizer synchronizer);
        void Push();
        void Pull();
    }
}