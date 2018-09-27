using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    [Obsolete("KP-11815")]
    public interface IDataExportProcessesService
    {
        DataExportProcessDetails GetAndStartOldestUnprocessedDataExport();

        void AddDataExport(DataExportProcessDetails details);

        DataExportProcessDetails[] GetRunningExportProcesses();

        DataExportProcessDetails[] GetAllProcesses();

        void FinishExportSuccessfully(string processId);

        void FinishExportWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExport(string processId);

        void ChangeStatusType(string processId, DataExportStatus status);
    }
}
