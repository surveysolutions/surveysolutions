using System;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] Questions { get; private set; }

        protected QuestionsPassiveEvent(Identity[] questions, DateTimeOffset originDate) : base(originDate)
        {
            this.Questions = questions?.ToArray() ?? new Identity[] {};
        }
    }
}
