using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportProcessesService
    {
        IDataExportProcessDetails GetAndStartOldestUnprocessedDataExport();

        string AddDataExport(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat, InterviewStatus? status = null);

        string AddParaDataExport(DataExportFormat exportFormat);

        IDataExportProcessDetails[] GetRunningExportProcesses();

        IDataExportProcessDetails[] GetAllProcesses();

        void FinishExportSuccessfully(string processId);

        void FinishExportWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExport(string processId);

        void DeleteProcess(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat, DataExportType exportType);
    }
}