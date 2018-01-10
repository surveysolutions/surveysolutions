using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.MoveUserToAnotherTeam
{
    public class MoveUserToAnotherTeamService : IMoveUserToAnotherTeamService
    {
        private readonly IAssignmentsService assignmentsService;
        private readonly HqUserManager userManager;
        private readonly ICommandService commandService;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader;

        public MoveUserToAnotherTeamService(
            IAssignmentsService assignmentsService, 
            HqUserManager userManager, 
            ICommandService commandService, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader)
        {
            this.assignmentsService = assignmentsService;
            this.userManager = userManager;
            this.commandService = commandService;
            this.interviewsReader = interviewsReader;
        }

        public async Task<MoveResult> Move(Guid userId, Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId,
            MoveUserToAnotherTeamMode moveRequestMode)
        {
            if (moveRequestMode == MoveUserToAnotherTeamMode.MoveAllToNewTeam)
            {
                return await MoveUserWithAllDataToANewTeam(userId, interviewerId, newSupervisorId, previousSupervisorId);
            }

            return await MoveUserAndAssignDataToOriginalSupervisor(userId, interviewerId, newSupervisorId, previousSupervisorId);
        }

        private async Task<MoveResult> MoveUserAndAssignDataToOriginalSupervisor(Guid userId, Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId)
        {
            var errors = new List<string>();
            
            var interviewIds = GetInterviewIds(interviewerId);
            foreach (var interviewId in interviewIds)
            {
                var moveInterviewToTeam = new MoveInterviewToTeam(interviewId, userId, previousSupervisorId, null, DateTime.UtcNow);
                ExecuteMoveInterviewToTeam(moveInterviewToTeam, errors, interviewId);
            }

            var assignmentIds = assignmentsService.GetAllAssignmentIds(interviewerId);
            foreach (var assignmentId in assignmentIds)
            {
                assignmentsService.Reassign(assignmentId, previousSupervisorId);
            }

            var moveUserResult = await userManager.MoveUserToAnotherTeamAsync(interviewerId, newSupervisorId, previousSupervisorId);
            if (!moveUserResult.Succeeded)
                errors.AddRange(moveUserResult.Errors);

            return new MoveResult(errors);
        }

        private async Task<MoveResult> MoveUserWithAllDataToANewTeam(Guid userId, Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId)
        {
            var errors = new List<string>();

            var interviewIds = GetInterviewIds(interviewerId);
            foreach (var interviewId in interviewIds)
            {
                var moveInterviewToTeam = new MoveInterviewToTeam(interviewId, userId, newSupervisorId, interviewerId, DateTime.UtcNow);
                ExecuteMoveInterviewToTeam(moveInterviewToTeam, errors, interviewId);
            }

            // there is no information about supervisor in assignment. no need to update anything

            var moveUserResult = await userManager.MoveUserToAnotherTeamAsync(interviewerId, newSupervisorId, previousSupervisorId);
            if (!moveUserResult.Succeeded)
                errors.AddRange(moveUserResult.Errors);

            return new MoveResult(errors);
        }

        private List<Guid> GetInterviewIds(Guid interviewerId)
        {
            return interviewsReader.Query(_ => _.Where(x => x.ResponsibleId == interviewerId).Select(x => x.InterviewId).ToList());
        }

        private void ExecuteMoveInterviewToTeam(MoveInterviewToTeam moveInterviewToTeam, List<string> errors, Guid interviewId)
        {
            try
            {
                commandService.Execute(moveInterviewToTeam);
            }
            catch (InterviewException exception)
            {
                errors.Add($"Error during re-assigning interview {interviewId}. " + exception.Message);
            }
            catch (Exception exception)
            {
                errors.Add($"Error during re-assigning interview {interviewId}. " + exception.Message);
            }
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
