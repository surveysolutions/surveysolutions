using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;

namespace WB.ServicesIntegration.Export
{
    public interface IExportService
    {
        Task<DataExportUpdateRequestResult> Regenerate(
            long processId,
            string archivePassword,
            string accessToken,
            string refreshToken);

        Task<DataExportUpdateRequestResult> RequestUpdate(
            string questionnaireId,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? from,
            DateTime? to,
            string? archivePassword,
            string? accessToken,
            string? refreshToken,
            ExternalStorageType? storageType,
            Guid? translationId,
            bool? includeMeta);

        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            string questionnaireId,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate);

        Task<List<long>> GetRunningExportJobs();

        Task<List<long>> GetAllJobsList();

        Task<HttpResponseMessage> DownloadArchive(long id, string archiveName);
        
        Task<bool> WasExportRecreated(long processId);

        Task<DataExportProcessView> GetJobsStatus(long processId);

        Task<IEnumerable<DataExportProcessView>> GetJobsByQuery(DataExportFormat? exportType, InterviewStatus? interviewStatus,
            string? questionnaireIdentity, DataExportJobStatus? exportStatus, bool? hasFile, int? limit = null, int? offset = null);

        Task<List<DataExportProcessView>> GetJobsStatuses(long[] processIds);

        Task DeleteProcess(long jobId);

        Task DeleteAll();

        Task<HttpContent> GetDdiArchive(string questionnaireId, string? archivePassword);

        Task<HttpResponseMessage> Health();

        Task<string> Version();

        Task<string> GetConnectivityStatus();

        Task DeleteTenant();

    }
}
