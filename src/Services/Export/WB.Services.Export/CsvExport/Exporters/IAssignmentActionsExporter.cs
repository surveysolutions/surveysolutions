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
    public interface IAssignmentActionsExporter
    {
        Task ExportAsync(List<int> assignmentIdsToExport, TenantInfo tenantInfo, string basePath, ExportProgress progress, CancellationToken cancellationToken);
        Task ExportAllAsync(TenantInfo tenantInfo, QuestionnaireIdentity questionnaireIdentity, string basePath,
            ExportProgress progress, CancellationToken cancellationToken);
        void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string folderPath);
        string AssignmentActionsFileName { get; }
        DoExportFileHeader[] ActionFileColumns { get; }
    }
}
