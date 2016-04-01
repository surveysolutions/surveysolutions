namespace WB.Core.Synchronization
{
    public interface ISyncPackagesProcessor
    {
        void ProcessNextSyncPackageBatchInParallel(int batchSize, int maxDegreeOfParallelism = 1);
    }
}  