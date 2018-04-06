using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsUpgradeService
    {
        void EnqueueUpgrade(QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo);

        void ReportProgress(QuestionnaireIdentity targetQuestionnaire, AssignmentUpgradeProgressDetails progressDetails);

        QueuedUpgrade DequeueUpgrade();
    }

    public class AssignmentUpgradeProgressDetails
    {
        public AssignmentUpgradeProgressDetails(QuestionnaireIdentity migrateFrom, 
            QuestionnaireIdentity migrateTo, 
            int totalAssignmentsToMigrate, 
            int assignmentsMigratedSuccessfuly,
            List<AssignmentUpgradeError> assignmentsMigratedWithError)
        {
            MigrateFrom = migrateFrom;
            MigrateTo = migrateTo;
            TotalAssignmentsToMigrate = totalAssignmentsToMigrate;
            AssignmentsMigratedSuccessfuly = assignmentsMigratedSuccessfuly;
            AssignmentsMigratedWithError = assignmentsMigratedWithError;
        }

        public QuestionnaireIdentity MigrateFrom { get; }
        public QuestionnaireIdentity MigrateTo { get; }

        public int TotalAssignmentsToMigrate { get; }
        public int AssignmentsMigratedSuccessfuly { get; }
        public List<AssignmentUpgradeError> AssignmentsMigratedWithError { get; }
    }

    public class AssignmentUpgradeError
    {
        public AssignmentUpgradeError(int assignmentId, string errorMessage)
        {
            AssignmentId = assignmentId;
            ErrorMessage = errorMessage;
        }

        public int AssignmentId { get; }

        public string ErrorMessage { get; }
    }
}
