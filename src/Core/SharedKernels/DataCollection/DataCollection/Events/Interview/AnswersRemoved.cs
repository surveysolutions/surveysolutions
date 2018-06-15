using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersRemoved : QuestionsPassiveEvent
    {
        public DateTime? RemoveTime { get; set; }

        public AnswersRemoved(Identity[] questions, DateTimeOffset originDate)
            : base(questions, originDate)
        {
            if (originDate != default(DateTimeOffset))
            {
                this.RemoveTime = originDate.UtcDateTime;
            }
        }
    }
}
