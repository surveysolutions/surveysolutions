using System;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Services.Processing
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetArchiveFilePathForExportedData(QuestionnaireId questionnaireId, DataExportFormat format,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);

        string GetExportDirectory();
        void MoveExportArchive(string tempArchivePath, string archiveName);
    }
}