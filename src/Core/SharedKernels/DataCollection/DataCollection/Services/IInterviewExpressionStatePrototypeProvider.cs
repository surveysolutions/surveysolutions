using System;
using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionStateV6 GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
