using System;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Jobs
{
    public interface IExportArchiveHandleService
    {
        Task<DataExportArchive> DownloadArchiveAsync(TenantInfo tenant, string archiveName,
            DataExportFormat dataExportFormat, InterviewStatus? status,
            DateTime? from, DateTime? to);

        Task ClearAllExportArchives(TenantInfo tenant);
    }
}