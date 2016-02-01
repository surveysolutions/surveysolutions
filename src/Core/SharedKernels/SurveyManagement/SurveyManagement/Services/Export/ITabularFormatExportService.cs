using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface ITabularFormatExportService
    {
        void ExportInterviewsInTabularFormat(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string basePath, IProgress<int> progress, CancellationToken cancellationToken);
        void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath);
        string[] GetTabularDataFilesFromFolder(string basePath);
    }
}