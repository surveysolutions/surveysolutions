using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam
{
    public class MoveInterviewerToAnotherTeamResult
    {
        public MoveInterviewerToAnotherTeamResult()
        {
            Errors = new List<string>();
        }

        public int InterviewsProcessed { get; set; } = 0;

        public int InterviewsProcessedWithErrors { get; set; } = 0;

        public int AssignmentsProcessed { get; set; } = 0;

        public int AssignmentsProcessedWithErrors { get; set; } = 0;

        public List<string> Errors { get; }
    }
}
