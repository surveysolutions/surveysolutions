using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    internal interface IMetadataExportService
    {
        string CreateAndGetDDIMetadataFileForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);
    }
}