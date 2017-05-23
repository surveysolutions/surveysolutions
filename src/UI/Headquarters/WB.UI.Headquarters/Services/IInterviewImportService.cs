using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Services
{
    public interface IInterviewImportService
    {
        InterviewImportStatus Status { get; }
        void ImportAssignments(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, Guid? supervisorId, Guid headquartersId);
        void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, bool isPanel, Guid? supervisorId, Guid headquartersId);
    }
}