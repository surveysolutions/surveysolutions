using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersRemoved : QuestionsPassiveEvent
    {
        public DateTime? RemoveTime { get; private set; }

        public AnswersRemoved(Identity[] questions, DateTime? removeTime)
            : base(questions)
        {
            this.RemoveTime = removeTime;
        }
    }
}
