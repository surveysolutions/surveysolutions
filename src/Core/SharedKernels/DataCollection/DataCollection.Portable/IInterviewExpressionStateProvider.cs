using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStateProvider
    {
        IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion);

        void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assembly);
    }
}
