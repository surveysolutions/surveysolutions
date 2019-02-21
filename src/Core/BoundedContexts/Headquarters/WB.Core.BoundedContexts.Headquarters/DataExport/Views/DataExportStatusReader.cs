using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly IExportServiceApi exportServiceApi;
        private readonly IExportFileNameService exportFileNameService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAssignmentsService assignmentsService;

        public DataExportStatusReader(IExportServiceApi exportServiceApi,
            IExportFileNameService exportFileNameService,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsService assignmentsService)
        {
            this.exportServiceApi = exportServiceApi;
            this.exportFileNameService = exportFileNameService;
            this.questionnaireStorage = questionnaireStorage;
            this.assignmentsService = assignmentsService;
        }

        public async Task<DataExportArchive> GetDataArchive(QuestionnaireIdentity questionnaireIdentity, DataExportFormat format,
            InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            var archiveFileName = exportFileNameService.GetQuestionnaireTitleWithVersion(questionnaireIdentity);
            var result = await exportServiceApi.DownloadArchive(questionnaireIdentity.ToString(), archiveFileName, format, status, from, to);

            if (result.StatusCode == HttpStatusCode.NotFound) return null;

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
            DataExportStatusView result = await exportServiceApi.GetDataExportStatusForQuestionnaireAsync(
                questionnaireIdentity.ToString(), status, fromDate, toDate);

            var binaryExport = result.DataExports.Where(x => x.DataExportFormat == DataExportFormat.Binary);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            var hasAssignmentWithAudioRecordingEnabled = assignmentsService.HasAssignmentWithAudioRecordingEnabled(questionnaireIdentity);
            foreach (var dataExportView in binaryExport)
            {
                if (!questionnaire.HasAnyMultimediaQuestion() &&
                    !hasAssignmentWithAudioRecordingEnabled)
                {
                    dataExportView.CanRefreshBeRequested = false;
                    dataExportView.HasAnyDataToBePrepared = false;
                }
            }

            return result;
        }
    }
}
