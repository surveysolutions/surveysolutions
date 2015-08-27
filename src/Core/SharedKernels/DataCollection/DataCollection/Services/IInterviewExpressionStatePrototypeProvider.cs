using System;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionStateV2 GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
