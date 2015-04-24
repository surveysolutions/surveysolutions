using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewExpressionStateUpgrader : IInterviewExpressionStateUpgrader
    {
        public IInterviewExpressionStateV2 UpgradeToLatestVersionIfNeeded(IInterviewExpressionState state)
        {
            return state as IInterviewExpressionStateV2 ?? new InterviewExpressionStateV1ToV2Adapter(state);
        }
    }
}
