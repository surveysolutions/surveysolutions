#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    internal class AssignmentsUpgrader : IAssignmentsUpgrader
    {
        private readonly IAssignmentsService assignments;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly IInScopeExecutor inScopeExecutor;

        public AssignmentsUpgrader(IAssignmentsService assignments,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsUpgradeService upgradeService,
            IInScopeExecutor inScopeExecutor)
        {
            this.assignments = assignments ?? throw new ArgumentNullException(nameof(assignments));
            this.questionnaireStorage =
                questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.upgradeService = upgradeService;
            this.inScopeExecutor = inScopeExecutor;
        }

        public void Upgrade(AssignmentsUpgradeProcess upgradeProcess, CancellationToken cancellation)
        {
            var migrateFrom = upgradeProcess.From;
            var migrateTo = upgradeProcess.To;
            var userId = upgradeProcess.UserId;
            var processId = upgradeProcess.ProcessId;

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
                        this.inScopeExecutor.Execute(sl =>
                        {
                            var singleUpgrader = sl.GetInstance<ISingleAssignmentUpgrader>();
                            singleUpgrader.Upgrade(assignmentId, targetQuestionnaire, userId, migrateTo);
                        });
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
