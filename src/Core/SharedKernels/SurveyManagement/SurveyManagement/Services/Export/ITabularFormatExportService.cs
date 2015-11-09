using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface ITabularFormatExportService
    {
        Task ExportInterviewsInTabularFormatAsync(QuestionnaireIdentity questionnaireIdentity, string basePath);
        Task ExportApprovedInterviewsInTabularFormatAsync(QuestionnaireIdentity questionnaireIdentity, string basePath);
        void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath);
        string[] GetTabularDataFilesFromFolder(string basePath);
    }
}