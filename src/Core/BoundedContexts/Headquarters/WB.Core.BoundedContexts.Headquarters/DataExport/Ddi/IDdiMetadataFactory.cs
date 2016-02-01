using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi
{
    internal interface IDdiMetadataFactory
    {
        string CreateDDIMetadataFileForQuestionnaireInFolder(Guid questionnaireId, long questionnaireVersion, string basePath);
    }
}