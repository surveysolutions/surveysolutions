using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade
{
    public interface IAssignmentsUpgrader
    {
        void Upgrade(QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo);
    }

    internal class AssignmentsUpgrader : IAssignmentsUpgrader
    {
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private readonly IInterviewImportService importService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAssignmentsUpgradeService upgradeService;

        public AssignmentsUpgrader(IPlainStorageAccessor<Assignment> assignments,
            IInterviewImportService importService,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsUpgradeService upgradeService
           )
        {
            this.assignments = assignments ?? throw new ArgumentNullException(nameof(assignments));
            this.importService = importService ?? throw new ArgumentNullException(nameof(importService));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.upgradeService = upgradeService;
        }

        public void Upgrade(QuestionnaireIdentity migrateFrom, QuestionnaireIdentity migrateTo)
        {
            var idsToMigrate = assignments.Query(_ =>
                _.Where(x => x.QuestionnaireId.Id == migrateFrom.Id && x.QuestionnaireId.Version == migrateFrom.Version).Select(x => x.Id).ToList());

            var targetQuestionnaire = this.questionnaireStorage.GetQuestionnaire(migrateTo, null);
            int migratedSuccessfully = 0;
            List<AssignmentUpgradeError> upgradeErrors = new List<AssignmentUpgradeError>();

            foreach (var assignmentId in idsToMigrate)
            {
                var oldAssignment = assignments.GetById(assignmentId);
                if (!oldAssignment.IsCompleted)
                {
                    var assignmentVerification = this.importService.VerifyAssignment(oldAssignment.Answers.GroupedByLevels(), targetQuestionnaire);
                    if (assignmentVerification.Status)
                    {
                        oldAssignment.Archive();

                        var newAssignment = new Assignment(migrateTo, oldAssignment.ResponsibleId, oldAssignment.Quantity);
                        newAssignment.SetAnswers(oldAssignment.Answers);
                        newAssignment.SetIdentifyingData(oldAssignment.IdentifyingData);
                        assignments.Store(newAssignment, null);
                        migratedSuccessfully++;
                    }
                    else
                    {
                        upgradeErrors.Add(new AssignmentUpgradeError(assignmentId, assignmentVerification.ErrorMessage));
                    }
                }
                else
                {
                    migratedSuccessfully++;
                }

                this.upgradeService.ReportProgress(migrateTo, new AssignmentUpgradeProgressDetails(migrateFrom, migrateTo, idsToMigrate.Count, migratedSuccessfully, upgradeErrors));
            }
        }
    }
}
