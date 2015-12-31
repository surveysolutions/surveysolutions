using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers
{
    public interface IInterviewImportService
    {
        InterviewImportStatus Status { get; }
        void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, byte[] fileBytes, Guid? supervisorId, Guid headquartersId);
        InterviewImportFileDescription GetDescriptionByFileWithInterviews(QuestionnaireIdentity questionnaireIdentity, byte[] fileBytes);
    }
}