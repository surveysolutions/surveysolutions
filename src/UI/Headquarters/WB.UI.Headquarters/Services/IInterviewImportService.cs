using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Services
{
    public interface IInterviewImportService
    {
        InterviewImportStatus Status { get; }
        void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, string sampleId, Guid? supervisorId, Guid headquartersId);
        InterviewImportFileDescription GetDescriptionByFileWithInterviews(QuestionnaireIdentity questionnaireIdentity, string sampleId);
    }
}