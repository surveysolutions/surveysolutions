#nullable enable
using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public class SingleAssignmentUpgrader : ISingleAssignmentUpgrader
    {
        private readonly IInScopeExecutor inScopeExecutor;

        public SingleAssignmentUpgrader(IInScopeExecutor inScopeExecutor)
        {
            this.inScopeExecutor = inScopeExecutor;
        }

        public void Upgrade(int assignmentId, IQuestionnaire targetQuestionnaire, Guid userId,
            QuestionnaireIdentity migrateTo)
        {
            this.inScopeExecutor.Execute(sl =>
            {
                IAssignmentFactory assignmentFactory = sl.GetInstance<IAssignmentFactory>();
                IInvitationService invitationService = sl.GetInstance<IInvitationService>();
                ICommandService commandService = sl.GetInstance<ICommandService>();
                IPreloadedDataVerifier importService = sl.GetInstance<IPreloadedDataVerifier>();
                IAssignmentsService assignments = sl.GetInstance<IAssignmentsService>();
                
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
            });
        }
    }
}
