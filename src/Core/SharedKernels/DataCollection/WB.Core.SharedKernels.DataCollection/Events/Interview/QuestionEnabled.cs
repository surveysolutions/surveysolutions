using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QuestionEnabled : QuestionPassiveEvent
    {
        public QuestionEnabled(Guid questionId, int[] propagationVector)
            : base(questionId, propagationVector) {}
    }
}