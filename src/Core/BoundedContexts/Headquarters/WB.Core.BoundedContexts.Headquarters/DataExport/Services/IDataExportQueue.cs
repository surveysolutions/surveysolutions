using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportQueue
    {
        string DeQueueDataExportProcessId();

        string EnQueueDataExportProcess(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string EnQueueApprovedDataExportProcess(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string EnQueueParaDataExportProcess(DataExportFormat exportFormat);

        IQueuedProcess GetDataExportProcess(string processId);

        IQueuedProcess[] GetRunningProcess();

        void FinishDataExportProcess(string processId);

        void FinishDataExportProcessWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExportProcess(string processId);
    }
}