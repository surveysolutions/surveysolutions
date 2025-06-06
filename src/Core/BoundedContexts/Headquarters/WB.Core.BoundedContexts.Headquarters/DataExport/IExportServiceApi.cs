﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IExportServiceApi
    {
        [Put("/api/v1/job/regenerate")]
        Task<DataExportUpdateRequestResult> Regenerate(
            long processId,
            string archivePassword,
            string accessToken,
            string refreshToken);

        [Put("/api/v1/job/generate")]
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
            bool? includeMeta,
            bool? paradataReduced);

        [Get("/api/v1/job/status")]
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            string questionnaireId,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate);

        [Get("/api/v1/job/running")]
        Task<List<long>> GetRunningExportJobs();

        [Get("/api/v1/job/all")]
        Task<List<long>> GetAllJobsList();

        [Get("/api/v1/job/{id}/download")]
        Task<HttpResponseMessage> DownloadArchive(long id, string archiveName);
        
        [Get("/api/v1/job/wasExportRecreated")]
        Task<bool> WasExportRecreated(long processId);

        [Get("/api/v1/job")]
        Task<DataExportProcessView> GetJobsStatus(long processId);

        [Get("/api/v1/job/byQuery")]
        Task<IEnumerable<DataExportProcessView>> GetJobsByQuery(DataExportFormat? exportType, InterviewStatus? interviewStatus,
            string? questionnaireIdentity, DataExportJobStatus? exportStatus, bool? hasFile, int? limit = null, int? offset = null);

        [Get("/api/v1/jobs")]
        Task<List<DataExportProcessView>> GetJobsStatuses([Query(CollectionFormat.Multi)]long[] processIds);

        [Delete("/api/v1/job/byId")]
        Task DeleteProcess(long jobId);

        [Delete("/api/v1/deleteArchives")]
        Task DeleteArchives();

        [Post("/api/v1/runRetentionPolicy")]
        Task RunRetentionPolicy(int? countToKeep = null, int? daysToKeep = null);
        
        [Get("/api/v1/ddi")]
        Task<HttpContent> GetDdiArchive(string questionnaireId, string? archivePassword);

        [Get("/.hc")]
        Task<HttpResponseMessage> Health();

        [Get("/.version")]
        Task<string> Version();

        [Get("/.connectivity")]
        Task<string> GetConnectivityStatus();

        [Delete("/api/v1/deleteTenant")]
        Task DropTenant();

        [Get("/api/v1/statusDeleteTenant")]
        Task<ExportServiceDropTenantStatus> DroppingTenantStatus();

        [Delete("/api/v1/deleteQuestionnaire")]
        Task DeleteQuestionnaire(string questionnaire);
    }
}
