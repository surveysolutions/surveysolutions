using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi
{
    public interface IDdiMetadataAccessor
    {
        string GetFilePathToDDIMetadata(QuestionnaireIdentity questionnaireId);
    }
}