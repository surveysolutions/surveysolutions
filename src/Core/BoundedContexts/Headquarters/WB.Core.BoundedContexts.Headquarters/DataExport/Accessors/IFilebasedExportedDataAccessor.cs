using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetFilePathToExportedDDIMetadata(QuestionnaireIdentity questionnaireId);

        string GetArchiveFilePathForExportedData(QuestionnaireIdentity questionnaireId, DataExportFormat format);
        string GetArchiveFilePathForExportedApprovedData(QuestionnaireIdentity questionnaireId, DataExportFormat format);
    }
}
