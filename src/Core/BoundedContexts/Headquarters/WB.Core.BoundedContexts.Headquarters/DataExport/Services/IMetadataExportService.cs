using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface IMetadataExportService
    {
        string CreateAndGetDDIMetadataFileForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath);
    }
}