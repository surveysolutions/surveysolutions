using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Jobs
{
    public interface IExportArchiveHandleService
    {
        Task<DataExportArchive?> DownloadArchiveAsync(ExportSettings settings, string questionnaireNamePrefixOverride);

        Task ClearExportArchives(TenantInfo tenant);
        
        Task RunRetentionPolicy(TenantInfo tenant, int? countToKeep, int? daysToKeep);
    }
}
