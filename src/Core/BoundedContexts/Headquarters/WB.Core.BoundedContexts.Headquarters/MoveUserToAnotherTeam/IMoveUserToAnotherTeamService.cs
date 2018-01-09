using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.MoveUserToAnotherTeam
{
    public enum MoveUserToAnotherTeamMode
    {
        ReassigntToOriginalSupervisor = 1,
        MoveAllToNewTeam = 2
    }
    public interface IMoveUserToAnotherTeamService
    {
        Task<MoveResult> Move(Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId,
            MoveUserToAnotherTeamMode moveRequestMode);
    }
}