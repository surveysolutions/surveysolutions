using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public interface IInterviewImportService
    {
        AssignmentImportStatus Status { get; }

        void ImportAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, Guid? supervisorId, Guid headquartersId, AssignmentImportType mode);

        void VerifyAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, string fileName);
    }
}