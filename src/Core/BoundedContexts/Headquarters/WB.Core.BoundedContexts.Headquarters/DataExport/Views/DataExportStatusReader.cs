using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly IExportServiceApi exportServiceApi;
        private readonly IExportFileNameService ExportFileNameService;

        public DataExportStatusReader(IExportServiceApi exportServiceApi,
            IExportFileNameService exportFileNameService)
        {
            this.exportServiceApi = exportServiceApi;
            this.ExportFileNameService = exportFileNameService;
        }

        public async Task<DataExportArchive> GetDataArchive(QuestionnaireIdentity questionnaireIdentity, DataExportFormat format,
            InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            var archiveFileName = ExportFileNameService.GetQuestionnaireTitleWithVersion(questionnaireIdentity);
            var result = await exportServiceApi.DownloadArchive(questionnaireIdentity.ToString(), archiveFileName, format, status, from, to);

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

        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var result = await exportServiceApi.GetDataExportStatusForQuestionnaireAsync(
                questionnaireIdentity.ToString(), status, fromDate, toDate);
            return result;
        }
    }
}
