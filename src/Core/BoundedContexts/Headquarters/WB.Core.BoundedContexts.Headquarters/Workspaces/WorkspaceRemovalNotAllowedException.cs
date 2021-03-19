using System;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class WorkspaceRemovalNotAllowedException : ApplicationException
    {
        public WorkspaceRemovalNotAllowedException()
        {
        }

        public WorkspaceRemovalNotAllowedException(int interviewsCount, int assignmentsCount)
        {
            InterviewsCount = interviewsCount;
            AssignmentsCount = assignmentsCount;
        }

        public int InterviewsCount { get; set; }
        public int AssignmentsCount { get; set; }
    }
}
