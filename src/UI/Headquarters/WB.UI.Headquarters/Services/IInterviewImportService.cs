using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Services
{
    public interface IInterviewImportService
    {
        InterviewImportStatus Status { get; }
        void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string interviewImportProcessId, bool isPanel, Guid? supervisorId, Guid headquartersId);
        
    }
}