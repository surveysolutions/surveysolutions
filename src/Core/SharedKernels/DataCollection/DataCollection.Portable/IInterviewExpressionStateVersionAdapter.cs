using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStateVersionAdapter
    {
        IInterviewExpressionStateV2 AdaptToV2(IInterviewExpressionState state);
    }
}
