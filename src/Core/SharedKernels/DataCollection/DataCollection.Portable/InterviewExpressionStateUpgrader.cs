using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;

namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewExpressionStateUpgrader : IInterviewExpressionStateUpgrader
    {
        public IInterviewExpressionStateV4 UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state)
        {
            var stateV4 = state as IInterviewExpressionStateV4;
            if (stateV4 != null)
                return stateV4;

            var stateV2 = state as IInterviewExpressionStateV2 ?? new InterviewExpressionStateV1ToV2Adapter(state);

            return new InterviewExpressionStateV2ToV4Adapter(stateV2) ;
        }
    }
}
