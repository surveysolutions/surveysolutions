using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
