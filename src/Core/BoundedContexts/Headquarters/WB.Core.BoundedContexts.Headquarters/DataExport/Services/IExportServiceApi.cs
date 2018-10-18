using System;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IExportServiceApi
    {
        [Put("/api/v1/job/generate")]
        Task RequestUpdate(
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
            [Query]string questionnaireId,
            [Query]InterviewStatus? status,
            [Query]DateTime? fromDate,
            [Query]DateTime? toDate);

        [Get("/api/v1/job/download")]
        Task<HttpResponseMessage> DownloadArchive(
            [Query] string questionnaireId,
            [Query] string archiveName,
            [Query] DataExportFormat format,
            [Query] InterviewStatus? status,
            [Query] DateTime? fromDate,
            [Query] DateTime? toDate);

        [Delete("/api/v1/job")]
        Task DeleteProcess([Query] string processId);

        [Delete("/api/v1/delete")]
        Task DeleteAll();

        [Get("/api/v1/ddi")]
        Task<HttpContent> GetDdiArchive([Query]string questionnaireId, [Query]string archivePassword);

        [Get("/.hc")]
        Task<HttpResponseMessage> Health();

        [Get("/.version")]
        Task<string> Version();
    }
}
