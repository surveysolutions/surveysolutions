using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;

namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewExpressionStateUpgrader : IInterviewExpressionStateUpgrader
    {
        public IInterviewExpressionStateV5 UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state)
        {
            var stateV5 = state as IInterviewExpressionStateV5;
            if (stateV5 != null)
                return stateV5;

            var stateV2 = state as IInterviewExpressionStateV2 ?? new InterviewExpressionStateV1ToV2Adapter(state);
            var stateV4 = stateV2 as IInterviewExpressionStateV4 ?? new InterviewExpressionStateV2ToV4Adapter(stateV2);

            return new InterviewExpressionStateV4ToV5Adapter(stateV4) ;
        }
    }
}
