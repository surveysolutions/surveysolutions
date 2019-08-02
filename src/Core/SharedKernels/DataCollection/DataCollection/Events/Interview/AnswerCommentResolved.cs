using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerCommentResolved : QuestionActiveEvent
    {
        public IList<Guid> Comments { get; }

        public AnswerCommentResolved(Guid userId, 
            Guid questionId, 
            RosterVector rosterVector, 
            DateTimeOffset originDate, 
            IList<Guid> comments)
            : base(userId, questionId, rosterVector, originDate)
        {
            Comments = comments;
        }
    }
}