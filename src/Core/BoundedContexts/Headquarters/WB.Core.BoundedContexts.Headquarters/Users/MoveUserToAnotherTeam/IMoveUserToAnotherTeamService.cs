using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam
{
    public enum MoveUserToAnotherTeamMode
    {
        ReassignToOriginalSupervisor = 1,
        MoveAllToNewTeam = 2
    }
    public interface IMoveUserToAnotherTeamService
    {
        Task<MoveInterviewerToAnotherTeamResult> Move(Guid userId, Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId,
            MoveUserToAnotherTeamMode moveRequestMode);

        MoveInterviewsToSupervisorResult MoveInterviewsToSupervisor(Guid userId, Guid interviewerId, Guid supervisorId);
    }
}