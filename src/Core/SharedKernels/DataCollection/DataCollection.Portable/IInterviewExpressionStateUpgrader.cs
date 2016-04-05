using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStateUpgrader
    {
        ILatestInterviewExpressionState UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state);
    }
}
