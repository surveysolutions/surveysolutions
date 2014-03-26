using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [Obsolete]
    public class QuestionDisabled : QuestionPassiveEvent
    {
        public QuestionDisabled(Guid questionId, decimal[] propagationVector)
            : base(questionId, propagationVector) {}
    }
}