using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentAnswersChanged : AssignmentEvent
    {
        public IList<InterviewAnswer> Answers { get; }

        public AssignmentAnswersChanged(Guid userId, DateTimeOffset originDate, IList<InterviewAnswer> answers) 
            : base(userId, originDate)
        {
            Answers = answers;
        }
    }
}
