using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IIndexedView : IView
    {
        int SortIndex { get; }
    }
}