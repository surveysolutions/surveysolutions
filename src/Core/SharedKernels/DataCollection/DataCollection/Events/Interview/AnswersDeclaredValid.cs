using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredValid : QuestionsPassiveEvent
    {
        public AnswersDeclaredValid(Identity[] questions, DateTimeOffset originDate)
            : base(questions, originDate) {}
    }
}
