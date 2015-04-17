using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewExpressionStateFactory : IInterviewExpressionStateFactory
    {
        public IInterviewExpressionStateV2 GetInterviewExpressionStateOfV2(IInterviewExpressionState state)
        {
            var v2 = state as IInterviewExpressionStateV2;
            if (v2 != null)
                return v2;

            return new InterviewExpressionStateV1AdaptedToV2(state);
        }
    }
}
