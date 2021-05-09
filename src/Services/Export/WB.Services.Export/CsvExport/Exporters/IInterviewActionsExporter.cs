using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IInterviewActionsExporter
    {
        Task ExportAsync(TenantInfo tenant, QuestionnaireIdentity questionnaireIdentity, List<Guid> interviewIdsToExport,
            string basePath, ExportProgress progress, CancellationToken cancellationToken = default);

        void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string folderPath);
        string InterviewActionsFileName { get; }
        DoExportFileHeader[] ActionFileColumns { get; }
    }
}
