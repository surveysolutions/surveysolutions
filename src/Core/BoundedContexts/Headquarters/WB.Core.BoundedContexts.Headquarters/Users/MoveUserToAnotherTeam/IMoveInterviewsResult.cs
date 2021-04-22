using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam
{
    public interface IMoveInterviewsResult
    {
        int InterviewsProcessed { get; set; }

        int InterviewsProcessedWithErrors { get; set; }
        List<string> Errors { get; }
    }
}