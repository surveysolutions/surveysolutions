using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QuestionsMarkedAsReadonly : QuestionsPassiveEvent
    {
        public QuestionsMarkedAsReadonly(Identity[] questions, DateTimeOffset originDate) 
            : base(questions, originDate) {}
    }
}
