using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewExpressionStateUpgrader : IInterviewExpressionStateUpgrader
    {
        public IInterviewExpressionStateV6 UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state)
        {
            var stateV6 = state as IInterviewExpressionStateV6;
            if (stateV6 != null)
                return stateV6;

            var stateV2 = state as IInterviewExpressionStateV2 ?? new InterviewExpressionStateV1ToV2Adapter(state);
            var stateV4 = stateV2 as IInterviewExpressionStateV4 ?? new InterviewExpressionStateV2ToV4Adapter(stateV2);
            var stateV5 = stateV4 as IInterviewExpressionStateV5 ?? new InterviewExpressionStateV4ToV5Adapter(stateV4);

            return new InterviewExpressionStateV5ToV6Adapter(stateV5) ;
        }
    }
}
