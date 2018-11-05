using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IDiagnosticsExporter
    {
        Task ExportAsync(List<Guid> interviewIdsToExport, string basePath, TenantInfo tenant,
            IProgress<int> exportInterviewsProgress, CancellationToken cancellationToken);

        void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string folderPath);
        string DiagnosticsFileName { get; }
        DoExportFileHeader[] DiagnosticsFileColumns { get; }
    }
}
