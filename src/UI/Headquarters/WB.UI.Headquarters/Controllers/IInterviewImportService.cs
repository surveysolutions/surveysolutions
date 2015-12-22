using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers
{
    public interface IInterviewImportService
    {
        InterviewImportStatus Status { get; }
        void ImportInterviews(InterviewImportFileDescription fileDescription, Guid? supervisorId);
        InterviewImportFileDescription GetDescriptionByFileWithInterviews(QuestionnaireIdentity questionnaireIdentity, byte[] fileBytes);
    }
}