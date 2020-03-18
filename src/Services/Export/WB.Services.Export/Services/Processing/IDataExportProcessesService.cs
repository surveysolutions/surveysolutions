using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportProcessesService
    {
        Task<long> AddDataExport(DataExportProcessArgs args);
        Task<DataExportProcessArgs[]> GetAllProcesses(TenantInfo tenant, bool runningOnly = true);
        void UpdateDataExportProgress(long processId, int progressInPercents, TimeSpan estimatedTime = default);
        void DeleteDataExport(long processId, string reason);
        void ChangeStatusType(long processId, DataExportStatus status);
        Task<DataExportProcessArgs> GetProcessAsync(long processId);
        Task<List<DataExportProcessArgs>> GetProcessesAsync(long[] processIds);
    }
}
