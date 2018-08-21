using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi
{
    internal interface IDdiMetadataFactory
    {
        string CreateDDIMetadataFileForQuestionnaireInFolder(QuestionnaireIdentity questionnaireId, string basePath);
    }
}
