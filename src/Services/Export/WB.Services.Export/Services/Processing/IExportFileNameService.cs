using System;

namespace WB.Services.Export.Services.Processing
{
    public interface IExportFileNameService
    {
        string GetFileNameForBatchUploadByQuestionnaire(string questionnaireFilename);
        string GetFileNameForDdiByQuestionnaire(string questionnaireFilename, string pathToDdiMetadata);

        string GetFileNameForTabByQuestionnaire(string questionnaireFilename, string pathToExportedData,
            DataExportFormat format, string statusSuffix, DateTime? fromDate = null, DateTime? toDate = null);

        string GetFileNameForAssignmentTemplate(string questionnaireFilename);
    }
}