using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewExpressionStateProvider
    {
        IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion);
        
        //void RemoveAssemblyFromCache(Guid questionnaireId, long questionnaireVersion);
    }
}
