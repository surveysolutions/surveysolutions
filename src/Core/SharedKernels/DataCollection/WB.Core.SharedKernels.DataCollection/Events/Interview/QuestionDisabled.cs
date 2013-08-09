using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QuestionDisabled : QuestionPassiveEvent
    {
        public QuestionDisabled(Guid questionId)
            : base(questionId) {}
    }
}