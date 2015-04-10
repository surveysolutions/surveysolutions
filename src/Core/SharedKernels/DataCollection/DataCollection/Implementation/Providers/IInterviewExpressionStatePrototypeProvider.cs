using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Providers
{
    internal interface IInterviewExpressionStatePrototypeProvider
    {
        IInterviewExpressionStateV2 GetExpressionState(Guid questionnaireId, long questionnaireVersion);
    }
}
