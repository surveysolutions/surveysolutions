using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    internal interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
