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
            string archiveName,
            string archivePassword,
            string accessToken,
            ExternalStorageType? storageType,
            string apiKey,
            [Header("Origin")] string tenantBaseUrl
        );

        [Get("/api/v1/job/status")]
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            [Query]string questionnaireId,
            [Query]string archiveName,
            [Query]InterviewStatus? status,
            [Query]DateTime? fromDate,
            [Query]DateTime? toDate,
            [Query]string apiKey,
            [Header("Origin")]string tenantBaseUrl);

        [Get("/api/v1/ddi")]
        Task<HttpContent> GetDdiArchive([Query]string questionnaireId,
            [Query]string archivePassword,
            [Query]string apiKey,
            [Header("Origin")]string tenantBaseUrl);
        
        [Get("/api/v1/job/download")]
        Task<HttpResponseMessage> DownloadArchive([Query] string questionnaireId,
            [Query] string archiveName,
            [Query] DataExportFormat format,
            [Query] InterviewStatus? status,
            [Query] DateTime? fromDate,
            [Query] DateTime? toDate,
            string apiKey,
            [Header("Origin")] string baseUrl);
    }
}
