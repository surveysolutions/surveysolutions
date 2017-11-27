using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportProcessesService
    {
        DataExportProcessDetails GetAndStartOldestUnprocessedDataExport();

        string AddDataExport(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat, InterviewStatus? status = null);

        DataExportProcessDetails[] GetRunningExportProcesses();

        DataExportProcessDetails[] GetAllProcesses();

        void FinishExportSuccessfully(string processId);

        void FinishExportWithError(string processId, Exception e);

        void UpdateDataExportProgress(string processId, int progressInPercents);

        void DeleteDataExport(string processId);

        void DeleteProcess(QuestionnaireIdentity questionnaire, DataExportFormat exportFormat);

        void ChangeStatusType(string processId, DataExportStatus status);
    }
}