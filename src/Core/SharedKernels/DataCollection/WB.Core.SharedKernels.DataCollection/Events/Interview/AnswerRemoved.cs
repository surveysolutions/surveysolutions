using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerRemoved : QuestionPassiveEvent
    {
        public AnswerRemoved(Guid questionId, int[] propagationVector)
            : base(questionId, propagationVector) {}
    }
}