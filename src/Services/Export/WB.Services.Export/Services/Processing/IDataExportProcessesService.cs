using System;
using System.Collections.Generic;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportProcessesService
    {
        string AddDataExport(DataExportProcessDetails args);

        IEnumerable<DataExportProcessDetails> GetRunningExportProcesses();

        DataExportProcessDetails[] GetAllProcesses();

        void FinishExportSuccessfully(string processId);

        void FinishExportWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExport(string processId);

        void ChangeStatusType(string processId, DataExportStatus status);
    }
}
