using System;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportProcessesService
    {
        Task<long> AddDataExport(DataExportProcessArgs args);
        Task<DataExportProcessArgs[]> GetAllProcesses(TenantInfo tenant);
        void UpdateDataExportProgress(long processId, int progressInPercents);
        void DeleteDataExport(long processId, string reason);
        void ChangeStatusType(long processId, DataExportStatus status);
    }
}
