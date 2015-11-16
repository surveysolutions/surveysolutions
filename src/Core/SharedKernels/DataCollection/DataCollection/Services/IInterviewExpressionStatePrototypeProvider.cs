using System;
using WB.Core.SharedKernels.DataCollection.V5;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionStateV5 GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
