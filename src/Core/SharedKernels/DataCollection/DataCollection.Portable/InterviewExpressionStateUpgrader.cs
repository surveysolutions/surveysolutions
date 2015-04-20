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
            var v2 = state as IInterviewExpressionStateV2;
            if (v2 != null)
                return v2;

            return new InterviewExpressionStateV1ToV2Adapter(state);
        }
    }
}
