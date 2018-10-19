using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IInterviewsExporter
    {
        Task ExportAsync(TenantInfo tenant, QuestionnaireExportStructure questionnaireExportStructure,
            QuestionnaireDocument questionnaire, List<InterviewToExport> interviewsToExport, string basePath,
            IProgress<int> progress, CancellationToken cancellationToken);
    }
}
