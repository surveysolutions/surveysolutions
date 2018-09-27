using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportProcessesService
    {
        [Obsolete("KP-11815")]
        DataExportProcessDetails GetAndStartOldestUnprocessedDataExport();

        Task AddDataExportAsync(string baseUrl, string apiKey, DataExportProcessDetails details);

        DataExportProcessDetails[] GetRunningExportProcesses();

        [Obsolete("KP-11815")]
        void FinishExportSuccessfully(string processId);

        [Obsolete("KP-11815")]
        void FinishExportWithError(string processId, Exception e);

        [Obsolete("KP-11815")]
        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExport(string processId);

        [Obsolete("KP-11815")]
        void ChangeStatusType(string processId, DataExportStatus status);
    }
}
