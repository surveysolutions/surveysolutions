#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    internal class AssignmentsUpgrader : IAssignmentsUpgrader
    {
        private readonly IAssignmentsService assignments;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly ISingleAssignmentUpgrader assignmentUpgrader;

        public AssignmentsUpgrader(IAssignmentsService assignments,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsUpgradeService upgradeService,
            ISingleAssignmentUpgrader assignmentUpgrader)
        {
            this.assignments = assignments ?? throw new ArgumentNullException(nameof(assignments));
            this.questionnaireStorage =
                questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.upgradeService = upgradeService;
            this.assignmentUpgrader = assignmentUpgrader;
        }

        public void Upgrade(Guid processId, Guid userId, QuestionnaireIdentity migrateFrom,
            QuestionnaireIdentity migrateTo, CancellationToken cancellation)
        {
            var idsToMigrate = assignments.GetAllAssignmentIdsForMigrateToNewVersion(migrateFrom);

            IQuestionnaire targetQuestionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(migrateTo, null);
            int migratedSuccessfully = 0;
            List<AssignmentUpgradeError> upgradeErrors = new List<AssignmentUpgradeError>();

            try
            {
                foreach (var assignmentId in idsToMigrate)
                {
                    cancellation.ThrowIfCancellationRequested();

                    try
                    {
                        this.assignmentUpgrader.Upgrade(assignmentId, targetQuestionnaire, userId, migrateTo);
                        migratedSuccessfully++;
                    }
                    catch (AssignmentUpgradeException e)
                    {
                        upgradeErrors.Add(new AssignmentUpgradeError(assignmentId, e.Message));
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
