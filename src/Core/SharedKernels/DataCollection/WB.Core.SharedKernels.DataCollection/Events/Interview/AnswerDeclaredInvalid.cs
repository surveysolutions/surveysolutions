using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerDeclaredInvalid : QuestionPassiveEvent
    {
        public AnswerDeclaredInvalid(Guid questionId, int[] propagationVector)
            : base(questionId, propagationVector) {}
    }
}