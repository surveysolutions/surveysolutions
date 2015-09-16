using System;
using WB.Core.SharedKernels.DataCollection.V4;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionStateV4 GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
