using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportProcessesService
    {
        IDataExportProcessDetails GetAndStartOldestUnprocessedDataExport();

        string AddAllDataExport(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat);

        string AddApprovedDataExport(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat);

        string AddParaDataExport(DataExportFormat exportFormat);

        IDataExportProcessDetails[] GetRunningExportProcesses();

        void FinishExportSuccessfully(string processId);

        void FinishExportWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExport(string processId);
    }
}