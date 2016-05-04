using System;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionStateV7 GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
