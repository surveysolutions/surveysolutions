using System;
using System.Threading.Tasks;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IExportServiceApi
    {
        [Get("/api/v1/job/status")]
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            [Query]string questionnaireId,
            [Query]string archiveName,
            [Query]InterviewStatus? status,
            [Query]DateTime? fromDate,
            [Query]DateTime? toDate,
            [Query]string apiKey,
            [Header("Origin")]string tenantBaseUrl);
    }
}
