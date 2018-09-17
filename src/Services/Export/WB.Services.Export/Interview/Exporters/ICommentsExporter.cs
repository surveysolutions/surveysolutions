using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Interview.Exporters
{
    public interface ICommentsExporter
    {
        Task ExportAsync(QuestionnaireExportStructure questionnaireExportStructure,
            List<Guid> interviewIdsToExport,
            string basePath,
            TenantInfo tenant,
            IProgress<int> progress,
            CancellationToken cancellationToken);
    }
}
