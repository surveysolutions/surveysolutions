using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam
{
    public class MoveInterviewsToSupervisorResult : IMoveInterviewsResult
    {
        public int InterviewsProcessed { get; set; } = 0;

        public int InterviewsProcessedWithErrors { get; set; } = 0;
        public List<string> Errors { get; } = new List<string>();
    }
}