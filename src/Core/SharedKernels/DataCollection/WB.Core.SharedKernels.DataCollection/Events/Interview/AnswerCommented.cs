using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerCommented : QuestionActiveEvent
    {
        public string Comment { get; private set; }

        public AnswerCommented(Guid userId, Guid questionId, string comment)
            : base(userId, questionId)
        {
            this.Comment = comment;
        }
    }
}