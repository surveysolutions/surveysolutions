using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentAnswers : AssignmentCommand
    {
        public List<InterviewAnswer> Answers { get; }

        public UpdateAssignmentAnswers(Guid assignmentId, Guid userId, List<InterviewAnswer> answers) : base(assignmentId, userId)
        {
            Answers = answers;
        }
    }
}
