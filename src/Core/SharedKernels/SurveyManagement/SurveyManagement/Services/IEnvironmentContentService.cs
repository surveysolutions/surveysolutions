using System;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IEnvironmentContentService
    {
        void CreateContentOfAdditionalFile(HeaderStructureForLevel headerStructureForLevel, string dataFileName, string basePath);
        string[] GetContentFilesForQuestionnaire(Guid questionnaireId, long version, string basePath);
    }
}