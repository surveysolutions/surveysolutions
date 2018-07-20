using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class QuestionsDisabled : QuestionsPassiveEvent
    {
        public QuestionsDisabled(Identity[] questions, DateTimeOffset originDate)
            : base(questions, originDate) {}
    }
}
