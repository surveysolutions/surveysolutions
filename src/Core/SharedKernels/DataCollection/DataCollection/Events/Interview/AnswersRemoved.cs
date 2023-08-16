using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersRemoved : QuestionsPassiveEvent
    {
        [Obsolete("Please use OriginDate property")]
        public DateTime? RemoveTime { get; set; }
        public Guid? UserId { get; set; }

        public AnswersRemoved(Guid? userId, Identity[] questions, DateTimeOffset originDate)
            : base(questions, originDate)
        {
            UserId = userId;
        }
    }
}
