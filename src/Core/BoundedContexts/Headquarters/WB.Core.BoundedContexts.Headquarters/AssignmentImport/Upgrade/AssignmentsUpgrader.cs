#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.Domain;
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
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly ILogger<AssignmentsUpgrader> logger;

        public AssignmentsUpgrader(IAssignmentsService assignments,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsUpgradeService upgradeService,
            IInScopeExecutor inScopeExecutor,
            ILogger<AssignmentsUpgrader> logger)
        {
            this.assignments = assignments ?? throw new ArgumentNullException(nameof(assignments));
            this.questionnaireStorage =
                questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.upgradeService = upgradeService;
            this.inScopeExecutor = inScopeExecutor;
            this.logger = logger;
        }

        public void Upgrade(Guid processId, Guid userId, QuestionnaireIdentity migrateFrom,
            QuestionnaireIdentity migrateTo, CancellationToken cancellation)
        {
            logger.LogInformation($"Upgrade assignments requested. From {migrateFrom} to {migrateTo}. Process: {processId}.");
            int migratedSuccessfully = 0;
            List<AssignmentUpgradeError> upgradeErrors = new List<AssignmentUpgradeError>();

            this.upgradeService.ReportProgress(processId,
                new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, 0,
                    migratedSuccessfully, upgradeErrors, AssignmentUpgradeStatus.InProgress));

            var idsToMigrate = assignments.GetAllAssignmentIdsForMigrateToNewVersion(migrateFrom);
            
            logger.LogInformation($"Assignments to upgrade: {idsToMigrate.Count}. Process: {processId}.");

            this.upgradeService.ReportProgress(processId,
                new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, idsToMigrate.Count,
                    migratedSuccessfully, upgradeErrors, AssignmentUpgradeStatus.InProgress));

            IQuestionnaire targetQuestionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(migrateTo, null);

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
