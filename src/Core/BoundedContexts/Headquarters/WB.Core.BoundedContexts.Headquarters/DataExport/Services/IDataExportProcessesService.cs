using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportProcessesService
    {
        IDataExportProcess GetAndStratOldestUnprocessedDataExport();

        string AddAllDataExport(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string AddApprovedDataExport(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string AddParaDataExport(DataExportFormat exportFormat);

        IDataExportProcess GetDataExport(string processId);

        IDataExportProcess[] GetRunningDataExports();

        void FinishDataExport(string processId);

        void FinishDataExportWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExport(string processId);
    }
}