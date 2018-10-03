using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IInterviewActionsExporter
    {
        Task ExportAsync(TenantInfo tenant, QuestionnaireId questionnaireIdentity, List<Guid> interviewIdsToExport,
            string basePath, IProgress<int> progress);

        void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string folderPath);
        string InterviewActionsFileName { get; }
        DoExportFileHeader[] ActionFileColumns { get; }
    }
}
