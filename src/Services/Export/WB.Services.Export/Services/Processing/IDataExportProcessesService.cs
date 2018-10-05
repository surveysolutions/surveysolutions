using System;
using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportProcessesService
    {
        Task<long> AddDataExport(DataExportProcessArgs args);

        Task<DataExportProcessArgs[]> GetAllProcesses(TenantInfo tenant);

        void FinishExportSuccessfully(long processId);

        void FinishExportWithError(TenantInfo tenant, string tag, Exception e);

        Task UpdateDataExportProgressAsync(TenantInfo tenant, string tag, int progressInPercents);

        Task DeleteDataExport(TenantInfo tenant, string tag);

        Task ChangeStatusTypeAsync(TenantInfo tenant, string tag, DataExportStatus status);
    }
}
