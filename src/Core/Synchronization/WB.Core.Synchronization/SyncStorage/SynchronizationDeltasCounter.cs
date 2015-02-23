using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SynchronizationDeltasCounter : IReadSideRepositoryEntity
    {
        public SynchronizationDeltasCounter(int countOfStoredDeltas)
        {
            this.CountOfStoredDeltas = countOfStoredDeltas;
        }

        public int CountOfStoredDeltas { get; private set; }
    }
}