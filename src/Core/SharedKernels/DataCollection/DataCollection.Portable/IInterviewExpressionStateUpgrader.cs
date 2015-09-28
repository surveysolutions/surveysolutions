using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStateUpgrader
    {
        IInterviewExpressionStateV4 UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state);
    }
}
