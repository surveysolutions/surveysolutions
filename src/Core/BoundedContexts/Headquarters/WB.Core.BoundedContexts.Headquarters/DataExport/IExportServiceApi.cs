﻿using System;
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
            string accessToken);

        [Put("/api/v1/job/generate")]
        Task<DataExportUpdateRequestResult> RequestUpdate(
            string questionnaireId,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? from,
            DateTime? to,
            string archivePassword,
            string accessToken,
            ExternalStorageType? storageType);

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

        [Get("/api/v1/job/download")]
        Task<HttpResponseMessage> DownloadArchive(
             string questionnaireId,
             string archiveName,
             DataExportFormat format,
             InterviewStatus? status,
             DateTime? fromDate,
             DateTime? toDate);

        [Get("/api/v1/job/wasExportRecreated")]
        Task<bool> WasExportRecreated(long processId);

        [Get("/api/v1/job")]
        Task<DataExportProcessView> GetJobsStatus(long processId);

        [Delete("/api/v1/job/byId")]
        Task DeleteProcess(long jobId);

        [Delete("/api/v1/deleteArchives")]
        Task DeleteAll();

        [Get("/api/v1/ddi")]
        Task<HttpContent> GetDdiArchive(string questionnaireId, string archivePassword);

        [Get("/.hc")]
        Task<HttpResponseMessage> Health();

        [Get("/.version")]
        Task<string> Version();
    }
}
