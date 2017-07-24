using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IInterviewImportService
    {
        AssignmentImportStatus Status { get; }

        void ImportAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, Guid? supervisorId, Guid headquartersId, AssignmentImportType mode);

        void VerifyAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, string fileName);

        AssignmentVerificationResult VerifyAssignment(List<InterviewAnswer>[] answersGroupedByLevels, IQuestionnaire questionnaire);
    }
}