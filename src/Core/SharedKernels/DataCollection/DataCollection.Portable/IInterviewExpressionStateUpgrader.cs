using WB.Core.SharedKernels.DataCollection.V5;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStateUpgrader
    {
        IInterviewExpressionStateV5 UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state);
    }
}
