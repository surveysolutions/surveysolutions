using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam
{
    public class MoveUserToAnotherTeamService : IMoveUserToAnotherTeamService
    {
        private readonly IAssignmentsService assignmentsService;
        private readonly IUserRepository userManager;
        private readonly ISystemLog auditLog;
        private readonly ICommandService commandService;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader;
        private readonly IWorkspaceContextAccessor workspaceContext;

        public MoveUserToAnotherTeamService(
            IAssignmentsService assignmentsService, 
            IUserRepository userManager, 
            ICommandService commandService,
            ISystemLog auditLog,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader,
            IWorkspaceContextAccessor workspaceContext)
        {
            this.assignmentsService = assignmentsService;
            this.userManager = userManager;
            this.commandService = commandService;
            this.auditLog = auditLog;
            this.interviewsReader = interviewsReader;
            this.workspaceContext = workspaceContext;
        }

        public async Task<MoveInterviewerToAnotherTeamResult> Move(Guid userId,
            Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId,
            MoveUserToAnotherTeamMode moveRequestMode)
        {
            if (moveRequestMode == MoveUserToAnotherTeamMode.MoveAllToNewTeam)
            {
                return await MoveUserWithAllDataToANewTeam(userId, interviewerId, newSupervisorId, previousSupervisorId);
            }

            return await MoveUserAndAssignDataToOriginalSupervisor(userId, interviewerId, newSupervisorId, previousSupervisorId);
        }

        private async Task<MoveInterviewerToAnotherTeamResult> MoveUserAndAssignDataToOriginalSupervisor(Guid userId, Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId)
        {
            MoveInterviewerToAnotherTeamResult result = MoveInterviewsToSupervisor(userId, interviewerId, previousSupervisorId);

            var assignmentIds = assignmentsService.GetAllAssignmentIds(interviewerId);
            foreach (var assignmentId in assignmentIds)
            {
                try
                {
                    result.AssignmentsProcessed++;
                    commandService.Execute(new ReassignAssignment(assignmentId, userId, previousSupervisorId, null));
                }
                catch (Exception exception)
                {
                    result.AssignmentsProcessedWithErrors++;
                    result.Errors.Add($"Error during re-assign of assignment {assignmentId}. " + exception.Message);
                }
            }

            var updateResult = await MoveToAnotherTeamAsync(interviewerId, newSupervisorId, previousSupervisorId);

            if (!updateResult.Succeeded)
                result.Errors.AddRange(updateResult.Errors.Select(x => x.Description));

            return result;
        }

        public MoveInterviewerToAnotherTeamResult MoveInterviewsToSupervisor(Guid userId, Guid interviewerId, Guid supervisorId)
        {
            var result = new MoveInterviewerToAnotherTeamResult();
            var interviewIds = GetInterviewIds(interviewerId);
            foreach (var interviewId in interviewIds)
            {
                var moveInterviewToTeam = new MoveInterviewToTeam(interviewId, userId, supervisorId, null);
                ExecuteMoveInterviewToTeam(moveInterviewToTeam, result, interviewId);
            }
            return result;
        }

        private async Task<Microsoft.AspNetCore.Identity.IdentityResult> MoveToAnotherTeamAsync(Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId)
        {
            var interviewer = await this.userManager.FindByIdAsync(interviewerId);
            var newSupervisor = await this.userManager.FindByIdAsync(newSupervisorId);
            var previousSupervisor = await this.userManager.FindByIdAsync(previousSupervisorId);
            var workspaceName = workspaceContext.CurrentWorkspace()!.Name;
            
            //interviewer.Profile.SupervisorId = newSupervisorId;
            var userWorkspace = interviewer.Workspaces
                .First(w => w.Workspace.Name == workspaceName && w.SupervisorId == previousSupervisorId);
            userWorkspace.ChangeSupervisorId(newSupervisorId);

            this.auditLog.UserMovedToAnotherTeam(interviewer.UserName, newSupervisor.UserName, previousSupervisor.UserName);

            var updateResult = await this.userManager.UpdateAsync(interviewer);
            return updateResult;
        }

        private async Task<MoveInterviewerToAnotherTeamResult> MoveUserWithAllDataToANewTeam(Guid userId, Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId)
        {
            var result = new MoveInterviewerToAnotherTeamResult();

            var interviewIds = GetInterviewIds(interviewerId);
            foreach (var interviewId in interviewIds)
            {
                var moveInterviewToTeam = new MoveInterviewToTeam(interviewId, userId, newSupervisorId, interviewerId);
                ExecuteMoveInterviewToTeam(moveInterviewToTeam, result, interviewId);
            }

            // there is no information about supervisor in assignment. no need to update anything

            var moveUserResult = await MoveToAnotherTeamAsync(interviewerId, newSupervisorId, previousSupervisorId);
            if (!moveUserResult.Succeeded)
                result.Errors.AddRange(moveUserResult.Errors.Select(x => x.Description));

            return result;
        }

        private List<Guid> GetInterviewIds(Guid interviewerId)
        {
            return interviewsReader.Query(_ => _.Where(x => x.ResponsibleId == interviewerId).Select(x => x.InterviewId).ToList());
        }

        private void ExecuteMoveInterviewToTeam(MoveInterviewToTeam moveInterviewToTeam, MoveInterviewerToAnotherTeamResult errors, Guid interviewId)
        {
            try
            {
                errors.InterviewsProcessed++;
                commandService.Execute(moveInterviewToTeam);
            }
            catch (InterviewException exception)
            {
                errors.InterviewsProcessedWithErrors++;
                errors.Errors.Add($"Error during re-assign of interview {interviewId}. " + exception.Message);
            }
            catch (Exception exception)
            {
                errors.InterviewsProcessedWithErrors++;
                errors.Errors.Add($"Error during re-assign of interview {interviewId}. " + exception.Message);
            }
        }
    }
}
