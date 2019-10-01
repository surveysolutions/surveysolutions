using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public interface IAssignmentsUpgrader
    {
        void Upgrade(Guid processId, Guid userId, QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo, CancellationToken cancellationToken);
    }

    internal class AssignmentsUpgrader : IAssignmentsUpgrader
    {
        private readonly IAssignmentsService assignments;
        private readonly IPreloadedDataVerifier importService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly IAssignmentFactory assignmentFactory;
        private readonly IInvitationService invitationService;
        private readonly ICommandService commandService;

        public AssignmentsUpgrader(IAssignmentsService assignments,
            IPreloadedDataVerifier importService,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsUpgradeService upgradeService,
            IAssignmentFactory assignmentFactory,
            IInvitationService invitationService,
            ICommandService commandService)
        {
            this.assignments = assignments ?? throw new ArgumentNullException(nameof(assignments));
            this.importService = importService ?? throw new ArgumentNullException(nameof(importService));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.upgradeService = upgradeService;
            this.assignmentFactory = assignmentFactory;
            this.invitationService = invitationService;
            this.commandService = commandService;
        }

        public void Upgrade(Guid processId, Guid userId, QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo, CancellationToken cancellation)
        { 
            var idsToMigrate = assignments.GetAllAssignmentIdsForMigrateToNewVersion(migrateFrom);

            var targetQuestionnaire = this.questionnaireStorage.GetQuestionnaire(migrateTo, null);
            int migratedSuccessfully = 0;
            List<AssignmentUpgradeError> upgradeErrors = new List<AssignmentUpgradeError>();

            try
            {
                foreach (var assignmentId in idsToMigrate)
                {
                    cancellation.ThrowIfCancellationRequested();

                    var oldAssignment = assignments.GetAssignment(assignmentId);
                    if (!oldAssignment.IsCompleted)
                    {
                        var assignmentVerification =
                            this.importService.VerifyWithInterviewTree(oldAssignment.Answers, null,
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

                            migratedSuccessfully++;
                        }
                        else
                        {
                            upgradeErrors.Add(new AssignmentUpgradeError(assignmentId,
                                assignmentVerification.ErrorMessage));
                        }
                    }
                    else
                    {
                        migratedSuccessfully++;
                    }

                    this.upgradeService.ReportProgress(processId,
                        new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, idsToMigrate.Count,
                            migratedSuccessfully, upgradeErrors, AssignmentUpgradeStatus.InProgress));
                }

                this.upgradeService.ReportProgress(processId,
                    new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, idsToMigrate.Count,
                        migratedSuccessfully, upgradeErrors, AssignmentUpgradeStatus.Done));
            }
            catch (OperationCanceledException)
            {
                this.upgradeService.ReportProgress(processId,
                    new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, idsToMigrate.Count,
                        migratedSuccessfully, upgradeErrors, AssignmentUpgradeStatus.Cancelled));
            }
        }
    }
}
