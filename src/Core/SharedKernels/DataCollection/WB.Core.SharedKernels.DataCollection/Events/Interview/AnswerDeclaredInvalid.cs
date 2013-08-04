using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerDeclaredInvalid : AnswerPassiveEvent
    {
        public AnswerDeclaredInvalid(Guid questionId)
            : base(questionId) {}
    }
}