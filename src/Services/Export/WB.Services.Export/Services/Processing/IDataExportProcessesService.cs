using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportProcessesService
    {
        Task<long> AddDataExport(DataExportProcessArgs args);
        Task<List<DataExportProcessArgs>> GetAllProcessesAsync(bool runningOnly = true, CancellationToken cancellationToken = default);
        void UpdateDataExportProgress(long processId, int progressInPercents, TimeSpan estimatedTime = default);
        void DeleteDataExport(long processId, string reason);
        void ChangeStatusType(long processId, DataExportStatus status);
        Task<DataExportProcessArgs?> GetProcessAsync(long processId);
        Task<List<DataExportProcessArgs>> GetProcessesAsync(long[] processIds);
    }
}
