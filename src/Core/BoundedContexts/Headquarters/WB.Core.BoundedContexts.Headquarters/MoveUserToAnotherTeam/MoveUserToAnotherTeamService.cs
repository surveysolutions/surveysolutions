using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.MoveUserToAnotherTeam
{
    public class MoveUserToAnotherTeamService : IMoveUserToAnotherTeamService
    {
        public async Task<MoveResult> Move(Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId,
            MoveUserToAnotherTeamMode moveRequestMode)
        {
            return new MoveResult(new List<string>());
        }
    }

    public class MoveResult
    {
        public MoveResult(List<string> errors)
        {
            Errors = errors;
        }

        public List<string> Errors { get; }
    }
}
