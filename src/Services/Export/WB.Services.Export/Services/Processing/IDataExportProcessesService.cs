using System;
using System.Collections.Generic;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportProcessesService
    {
        DataExportProcessDetails GetAndStartOldestUnprocessedDataExport();

        string AddDataExport(DataExportProcessDetails args);

        IEnumerable<DataExportProcessDetails> GetRunningExportProcesses(TenantInfo tenant);

        DataExportProcessDetails[] GetAllProcesses(TenantInfo tenant);

        void FinishExportSuccessfully(string processId);

        void FinishExportWithError(string processId, Exception e);

        void UpdateDataExportProgress(TenantInfo tenant, string processId, int progressInPercents);

        void DeleteDataExport(TenantInfo tenant, string processId);

        void ChangeStatusType(TenantInfo tenant, string processId, DataExportStatus status);
    }
}
