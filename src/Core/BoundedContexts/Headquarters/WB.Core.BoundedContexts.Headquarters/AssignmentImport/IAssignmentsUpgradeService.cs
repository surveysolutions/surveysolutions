﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IAssignmentsUpgradeService
    {
        Task EnqueueUpgrade(Guid processId, Guid userId, 
            QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo, CancellationToken token = default);

        void ReportProgress(Guid processId, AssignmentUpgradeProgressDetails progressDetails);
        
        AssignmentUpgradeProgressDetails Status(Guid processId);

        CancellationToken GetCancellationToken(Guid processId);

        void StopProcess(Guid processId);
    }

    public class AssignmentUpgradeProgressDetails
    {
        public AssignmentUpgradeProgressDetails(QuestionnaireIdentity migrateFrom, 
            QuestionnaireIdentity migrateTo, 
            int totalAssignmentsToMigrate, 
            int assignmentsMigratedSuccessfully,
            List<AssignmentUpgradeError> assignmentsMigratedWithError,
            AssignmentUpgradeStatus status)
        {
            MigrateFrom = migrateFrom;
            MigrateTo = migrateTo;
            TotalAssignmentsToMigrate = totalAssignmentsToMigrate;
            AssignmentsMigratedSuccessfully = assignmentsMigratedSuccessfully;
            AssignmentsMigratedWithError = assignmentsMigratedWithError ?? new List<AssignmentUpgradeError>();
            Status = status;
        }

        public QuestionnaireIdentity MigrateFrom { get; }
        public QuestionnaireIdentity MigrateTo { get; }

        public int TotalAssignmentsToMigrate { get; }
        public int AssignmentsMigratedSuccessfully { get; }
        public List<AssignmentUpgradeError> AssignmentsMigratedWithError { get; }
        public AssignmentUpgradeStatus Status { get; }
    }

    public enum AssignmentUpgradeStatus
    {
        Queued = 1,
        InProgress = 2,
        Done = 3,
        Cancelled = 4
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
