using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IDiagnosticsExporter
    {
        Task ExportAsync(List<Guid> interviewIdsToExport, string basePath, TenantInfo tenant,
            Progress<int> exportInterviewsProgress, CancellationToken cancellationToken);
    }
}
