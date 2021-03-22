using System;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class WorkspaceRemovalNotAllowedException : ApplicationException
    {
        public WorkspaceRemovalNotAllowedException()
        {
        }

        public WorkspaceRemovalNotAllowedException(int interviewsCount,
            int assignmentsCount,
            int interviewersCount)
        {
            InterviewersCount = interviewersCount;
            InterviewsCount = interviewsCount;
            AssignmentsCount = assignmentsCount;
        }

        public int InterviewersCount { get; }
        public int InterviewsCount { get; set; }
        public int AssignmentsCount { get; set; }
    }
}
