using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface ISyncPackage : IView
    {
        string PackageId { get; set; }
    }

    public interface IOrderableSyncPackage : ISyncPackage
    {
        long SortIndex { get; set; }
    }
}