using System.Collections.Generic;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IInterviewErrorsExporter
    {
        void WriteHeader(bool hasAtLeastOneRoster, int maxRosterDepth, string errorsExportFilePath);
        void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath);
        List<string[]> Export(QuestionnaireExportStructure exportStructure, QuestionnaireDocument questionnaire, List<InterviewEntity> entitiesToExport, string path, string interviewKey);
    }
}
