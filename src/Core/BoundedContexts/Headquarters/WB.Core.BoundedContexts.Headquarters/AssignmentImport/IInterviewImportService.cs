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

        void ImportAssignments(QuestionnaireIdentity questionnaireIdentity, Guid? supervisorId, Guid headquartersId, AssignmentImportType mode, bool shouldSkipInterviewCreation);

        void VerifyAssignments(QuestionnaireIdentity questionnaireIdentity);

        AssignmentVerificationResult VerifyAssignment(List<InterviewAnswer>[] answersGroupedByLevels, IQuestionnaire questionnaire);
    }
}