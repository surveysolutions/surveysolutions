using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStateFactory
    {
        IInterviewExpressionStateV2 GetInterviewExpressionStateOfV2(IInterviewExpressionState state);
    }
}
