using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly InterviewDataExportSettings settings;
        private readonly IExportFileNameService ExportFileNameService;

        public DataExportStatusReader(InterviewDataExportSettings settings, 
            IExportFileNameService exportFileNameService)
        {
            this.settings = settings;
            this.ExportFileNameService = exportFileNameService;
        }

        public async Task<DataExportArchive> GetDataArchive(
            string baseUrl, string apiKey,
            QuestionnaireIdentity questionnaireIdentity, DataExportFormat format,
            InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            var api = RestService.For<IExportServiceApi>(settings.ExportServiceUrl);
            var archiveFileName = ExportFileNameService.GetQuestionnaireTitleWithVersion(questionnaireIdentity);
            var result = await api.DownloadArchive(questionnaireIdentity.ToString(), archiveFileName, format, status, from, to, apiKey, baseUrl);

            result.EnsureSuccessStatusCode();

            if(result.Headers.TryGetValues("NewLocation", out var values))
            {
                return new DataExportArchive
                {
                    Redirect = values.First()
                };
            }

            return new DataExportArchive
            {
                FileName = result.Content.Headers.ContentDisposition.FileName,
                Data = await result.Content.ReadAsStreamAsync()
            };
        }

        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(string baseUrl,
            string apiKey,
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var api = RestService.For<IExportServiceApi>(settings.ExportServiceUrl);
            var archiveFileName = ExportFileNameService.GetQuestionnaireTitleWithVersion(questionnaireIdentity);
            var result = await api.GetDataExportStatusForQuestionnaireAsync(questionnaireIdentity.ToString(),
                archiveFileName, status, fromDate, toDate, apiKey, baseUrl);
            return result;
        }
    }
}
