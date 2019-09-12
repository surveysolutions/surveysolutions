﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public interface IAssignmentsUpgrader
    {
        void Upgrade(Guid processId, QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo, CancellationToken cancellationToken);
    }

    internal class AssignmentsUpgrader : IAssignmentsUpgrader
    {
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private readonly IPreloadedDataVerifier importService;
        private readonly IQuestionnaireStorage questionnaireStorage;
       private readonly IAssignmentsUpgradeService upgradeService;
       private readonly IAssignmentFactory assignmentFactory;
       private readonly IInvitationService invitationService;

       public AssignmentsUpgrader(IPlainStorageAccessor<Assignment> assignments,
            IPreloadedDataVerifier importService,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsUpgradeService upgradeService,
            IAssignmentFactory assignmentFactory,
            IInvitationService invitationService
           )
        {
            this.assignments = assignments ?? throw new ArgumentNullException(nameof(assignments));
            this.importService = importService ?? throw new ArgumentNullException(nameof(importService));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.upgradeService = upgradeService;
            this.assignmentFactory = assignmentFactory;
            this.invitationService = invitationService;
        }

        public void Upgrade(Guid processId, QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo, CancellationToken cancellation)
        { 
            var idsToMigrate = assignments.Query(_ =>
                _.Where(x => x.QuestionnaireId.Id == migrateFrom.Id && x.QuestionnaireId.Version == migrateFrom.Version && !x.Archived)
                    .Select(x => x.Id).ToList());

            var targetQuestionnaire = this.questionnaireStorage.GetQuestionnaire(migrateTo, null);
            int migratedSuccessfully = 0;
            List<AssignmentUpgradeError> upgradeErrors = new List<AssignmentUpgradeError>();

            try
            {
                foreach (var assignmentId in idsToMigrate)
                {
                    cancellation.ThrowIfCancellationRequested();

                    var oldAssignment = assignments.GetById(assignmentId);
                    if (!oldAssignment.IsCompleted)
                    {
                        var assignmentVerification =
                            this.importService.VerifyWithInterviewTree(oldAssignment.Answers, null,
                                targetQuestionnaire);
                        if (assignmentVerification == null)
                        {
                            oldAssignment.Archive();

                            var newAssignment = assignmentFactory.CreateAssignment(migrateTo, 
                                oldAssignment.ResponsibleId,
                                oldAssignment.InterviewsNeeded, 
                                oldAssignment.Email, 
                                oldAssignment.Password, 
                                oldAssignment.WebMode,
                                oldAssignment.IsAudioRecordingEnabled,
                                oldAssignment.Comments);

                            newAssignment.SetAnswers(oldAssignment.Answers.ToList());
                            var newIdentifyingData = new List<IdentifyingAnswer>();
                            foreach (var oldIdentifyingQuestion in oldAssignment.IdentifyingData)
                            {
                                newIdentifyingData.Add(IdentifyingAnswer.Create(newAssignment,
                                    targetQuestionnaire,
                                    oldIdentifyingQuestion.Answer,
                                    oldIdentifyingQuestion.Identity,
                                    oldIdentifyingQuestion.VariableName));
                            }

                            newAssignment.SetIdentifyingData(newIdentifyingData);
                            newAssignment.SetProtectedVariables(oldAssignment.ProtectedVariables);
                            assignments.Store(newAssignment, null);

                            invitationService.MigrateInvitationToNewAssignment(assignmentId, newAssignment.Id);
                            oldAssignment.UpdateMode(false);

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
