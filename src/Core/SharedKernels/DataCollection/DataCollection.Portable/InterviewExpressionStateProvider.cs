using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public class InterviewExpressionStateProvider : IInterviewExpressionStateProvider
    {
        public IInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
        {
            return new StronglyTypedInterviewEvaluator();
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assembly)
        {
        }
    }
}
