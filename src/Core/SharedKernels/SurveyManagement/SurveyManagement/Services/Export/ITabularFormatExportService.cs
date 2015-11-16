using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface ITabularFormatExportService
    {
        void ExportInterviewsInTabularFormat(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress);
        void ExportApprovedInterviewsInTabularFormat(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress);
        void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath);
        string[] GetTabularDataFilesFromFolder(string basePath);
    }
}