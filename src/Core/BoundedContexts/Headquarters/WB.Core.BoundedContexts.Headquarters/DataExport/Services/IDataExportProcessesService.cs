using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportProcessesService
    {
        IDataExportProcess GetOldestUnprocessedDataExportProcess();

        string AddDataExportProcess(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string AddApprovedDataExportProcess(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string AddParaDataExportProcess(DataExportFormat exportFormat);

        IDataExportProcess GetDataExportProcess(string processId);

        IDataExportProcess[] GetRunningProcess();

        void FinishDataExportProcess(string processId);

        void FinishDataExportProcessWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExportProcess(string processId);
    }
}