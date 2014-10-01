using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    public interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
