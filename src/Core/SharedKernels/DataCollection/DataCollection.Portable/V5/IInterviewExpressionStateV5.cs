using System;
using WB.Core.SharedKernels.DataCollection.V4;

namespace WB.Core.SharedKernels.DataCollection.V5
{
    public interface IInterviewExpressionStateV5: IInterviewExpressionStateV4
    {
        void UpdateYesNoAnswer(Guid questionId, decimal[] propagationVector, YesNoAnswersOnly answers);

        new IInterviewExpressionStateV5 Clone();
    }
}