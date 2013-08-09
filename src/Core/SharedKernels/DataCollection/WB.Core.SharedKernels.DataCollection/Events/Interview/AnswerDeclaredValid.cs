using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerDeclaredValid : QuestionPassiveEvent
    {
        public AnswerDeclaredValid(Guid questionId)
            : base(questionId) {}
    }
}