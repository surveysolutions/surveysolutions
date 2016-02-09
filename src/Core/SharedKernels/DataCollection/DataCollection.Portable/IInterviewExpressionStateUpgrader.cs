using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStateUpgrader
    {
        IInterviewExpressionStateV6 UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state);
    }
}
