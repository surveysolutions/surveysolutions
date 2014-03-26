using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [Obsolete]
    public class AnswerRemoved : QuestionPassiveEvent
    {
        public AnswerRemoved(Guid questionId, decimal[] propagationVector)
            : base(questionId, propagationVector) {}
    }
}