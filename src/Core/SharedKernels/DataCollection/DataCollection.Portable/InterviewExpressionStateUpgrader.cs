using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewExpressionStateUpgrader : IInterviewExpressionStateUpgrader
    {
        public IInterviewExpressionStateV7 UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state)
        {
            var stateV7 = state as IInterviewExpressionStateV7;
            if (stateV7 != null)
                return stateV7;

            var stateV2 = state as IInterviewExpressionStateV2 ?? new InterviewExpressionStateV1ToV2Adapter(state);
            var stateV4 = stateV2 as IInterviewExpressionStateV4 ?? new InterviewExpressionStateV2ToV4Adapter(stateV2);
            var stateV5 = stateV4 as IInterviewExpressionStateV5 ?? new InterviewExpressionStateV4ToV5Adapter(stateV4);
            var stateV6 = stateV5 as IInterviewExpressionStateV6 ?? new InterviewExpressionStateV5ToV6Adapter(stateV5);

            return new InterviewExpressionStateV6ToV7Adapter(stateV6) ;
        }
    }
}
