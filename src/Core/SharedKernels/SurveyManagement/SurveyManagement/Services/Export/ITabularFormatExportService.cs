using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface ITabularFormatExportService
    {
        Task ExportInterviewsInTabularFormatAsync(Guid questionnaireId, long questionnaireVersion, string basePath);
        Task ExportApprovedInterviewsInTabularFormatAsync(Guid questionnaireId, long questionnaireVersion, string basePath);
        void CreateHeaderStructureForPreloadingForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);
    }
}