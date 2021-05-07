#nullable enable
using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public class SingleAssignmentUpgrader : ISingleAssignmentUpgrader
    {
        private readonly IAssignmentFactory assignmentFactory;
        private readonly IInvitationService invitationService;
        private readonly ICommandService commandService;
        private readonly IPreloadedDataVerifier importService;
        private readonly IAssignmentsService assignments;

        public SingleAssignmentUpgrader(
            IAssignmentFactory assignmentFactory,
            IInvitationService invitationService,
            ICommandService commandService,
            IPreloadedDataVerifier importService,
            IAssignmentsService assignments
        )
        {
            this.assignmentFactory = assignmentFactory;
            this.invitationService = invitationService;
            this.commandService = commandService;
            this.importService = importService;
            this.assignments = assignments;
        }

        public void Upgrade(int assignmentId, IQuestionnaire targetQuestionnaire, Guid userId,
            QuestionnaireIdentity migrateTo)
        {
            var oldAssignment = assignments.GetAssignment(assignmentId);
            if (!oldAssignment.IsCompleted)
            {
                var assignmentVerification =
                    importService.VerifyWithInterviewTree(oldAssignment.Answers, null,
                        targetQuestionnaire);
                if (assignmentVerification == null)
                {
                    var newAssignment = assignmentFactory.CreateAssignment(
                        userId,
                        migrateTo,
                        oldAssignment.ResponsibleId,
                        oldAssignment.InterviewsNeeded,
                        oldAssignment.Email,
                        oldAssignment.Password,
                        oldAssignment.WebMode,
                        oldAssignment.AudioRecording,
                        oldAssignment.Answers.ToList(),
                        oldAssignment.ProtectedVariables,
                        oldAssignment.Comments);

                    invitationService.MigrateInvitationToNewAssignment(assignmentId, newAssignment.Id);
                    commandService.Execute(new UpgradeAssignmentCommand(oldAssignment.PublicKey, userId));
                }
                else
                {
                    throw new AssignmentUpgradeException(assignmentVerification.ErrorMessage);
                }
            }
        }
    }
}
