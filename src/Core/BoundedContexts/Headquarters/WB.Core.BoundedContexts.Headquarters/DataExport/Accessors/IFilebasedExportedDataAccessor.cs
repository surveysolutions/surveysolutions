using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetArchiveFilePathForExportedData(QuestionnaireIdentity questionnaireId, DataExportFormat format,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);

        string GetExportDirectory();
        void MoveExportArchive(string tempArchivePath, string archiveName);
    }
}
