using System.IO;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers
{
    public interface IInterviewImportService
    {
        void ImportInterviews(QuestionnaireIdentity questionnaireIdentity, Stream zipOrCsvFileStream);
        void ImportInterviewsWithRosters(QuestionnaireIdentity questionnaireIdentity, Stream zipFileStream);
        InterviewImportStatus Status { get; }
    }
}