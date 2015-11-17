using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportProcessesService
    {
        IDataExportProcessDetails GetAndStartOldestUnprocessedDataExport();

        string AddAllDataExport(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string AddApprovedDataExport(Guid questionnaireId, long questionnaireVersion, DataExportFormat exportFormat);

        string AddParaDataExport(DataExportFormat exportFormat);

        IDataExportProcessDetails[] GetRunningDataExports();

        void FinishExportSuccessfully(string processId);

        void FinishExportWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExport(string processId);
    }
}