using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportService
    {
        string DeQueueDataExportProcessId();

        string EnQueueDataExportProcess(Guid questionnaireId, long questionnaireVersion, DataExportType exportType);

        DataExportProcessDto GetDataExportProcess(string processId);

        void FinishDataExportProcess(string processId);

        void FinishDataExportProcessWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);
    }
}