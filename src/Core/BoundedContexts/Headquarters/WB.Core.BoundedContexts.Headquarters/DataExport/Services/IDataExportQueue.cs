using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportQueue
    {
        string DeQueueDataExportProcessId();

        string EnQueueDataExportProcess(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string EnQueueParaDataExportProcess(DataExportFormat exportFormat);

        DataExportProcessDto GetDataExportProcess(string processId);

        void FinishDataExportProcess(string processId);

        void FinishDataExportProcessWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExportProcess(string processId);
    }
}