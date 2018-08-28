using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class SelectResponsibleForAssignmentArgs
    {
        public Guid? InterviewId { get; }
        public int? AssignmentId { get; }

        public SelectResponsibleForAssignmentArgs(Guid interviewId)
        {
            this.InterviewId = interviewId;
        }

        public SelectResponsibleForAssignmentArgs(int assignmentId)
        {
            this.AssignmentId = assignmentId;
        }
    }
}
