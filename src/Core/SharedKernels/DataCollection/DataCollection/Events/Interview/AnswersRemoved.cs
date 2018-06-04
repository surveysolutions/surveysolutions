using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersRemoved : QuestionsPassiveEvent
    {
        public DateTime? RemoveTime { get; private set; }

        public AnswersRemoved(Identity[] questions, DateTimeOffset originDate, DateTime? removeTime = null)
            : base(questions, originDate)
        {
            if (originDate != default(DateTimeOffset))
            {
                this.RemoveTime = originDate.UtcDateTime;
            }
            else if (removeTime != null && removeTime != default(DateTime))
            {
                this.RemoveTime = removeTime;
            }
        }
    }
}
