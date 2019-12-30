using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native;
// using WB.Infrastructure.Native.Logging.Slack;

namespace WB.Core.BoundedContexts.Headquarters.Services.Export
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly IExportServiceApi exportServiceApi;
        private readonly IExportFileNameService exportFileNameService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAssignmentsService assignmentsService;
        
        // TODO: Restore SLACK notifications
        //private readonly ISlackApiClient slackApiClient;
        private readonly ILogger logger;

        public DataExportStatusReader(
            IExportServiceApi exportServiceApi,
            IExportFileNameService exportFileNameService,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentsService assignmentsService,
            
            // TODO: Restore SLACK notifications
            // ISlackApiClient slackApiClient,
            ILoggerProvider loggerProvider)
        {
            this.exportServiceApi = exportServiceApi;
            this.exportFileNameService = exportFileNameService;
            this.questionnaireStorage = questionnaireStorage;
            this.assignmentsService = assignmentsService;
            // TODO: Restore SLACK notifications
            //    this.slackApiClient = slackApiClient;
            this.logger = loggerProvider.GetForType(this.GetType());
        }

        public async Task<DataExportArchive> GetDataArchive(QuestionnaireIdentity questionnaireIdentity,
            DataExportFormat format,
            InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            var archiveFileName = exportFileNameService.GetQuestionnaireTitleWithVersion(questionnaireIdentity);
            var result = await exportServiceApi.DownloadArchive(questionnaireIdentity.ToString(), archiveFileName,
                format, status, from, to);

            if (result.StatusCode == HttpStatusCode.NotFound) return null;

            result.EnsureSuccessStatusCode();

            if (result.Headers.TryGetValues(@"NewLocation", out var values))
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

        public Task<ExportDataAvailabilityView> GetDataAvailabilityAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            if (questionnaire == null)
                return Task.FromResult<ExportDataAvailabilityView>(null);

            var hasAssignmentWithAudioRecordingEnabled = assignmentsService.HasAssignmentWithAudioRecordingEnabled(questionnaireIdentity);
            return Task.FromResult(new ExportDataAvailabilityView
            {
                HasBinaryData = questionnaire.HasAnyMultimediaQuestion() || hasAssignmentWithAudioRecordingEnabled,
                HasInterviews = true
            });
        }

        public async Task<DataExportProcessView> GetProcessStatus(long id)
        {
            DataExportProcessView processView = await this.exportServiceApi.GetJobsStatus(id);
            if (processView == null)
            {
                return null;
            }

            if (processView.Error != null)
            {
                switch (processView.Error.Type)
                {
                    case DataExportError.Canceled: processView.Error.Message = Resources.DataExport.Error_Canceled; break;
                    case DataExportError.NotEnoughExternalStorageSpace: processView.Error.Message = Resources.DataExport.Error_NotEnoughExternalStorageSpace; break;
                    default: processView.Error.Message = Resources.DataExport.Error_Unhandled; break;
                }
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(processView.QuestionnaireIdentity, null);
            if (questionnaire == null)
            {
                return null;
            }

            processView.Title = questionnaire.Title;
            return processView;
        }

        public async Task<bool> WasExportFileRecreated(long processId)
        {
            return await exportServiceApi.WasExportRecreated(processId);
        }

        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                DataExportStatusView result = await exportServiceApi.GetDataExportStatusForQuestionnaireAsync(
                    questionnaireIdentity.ToString(), status, fromDate, toDate);

                var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

                if (questionnaire == null)
                {
                    return new DataExportStatusView() { Success = false };
                }

                // TODO: Restore SLACK notifications
                //await slackApiClient.SendMessageAsync(new SlackFatalMessage
                //{
                //    Color = SlackColor.Good,
                //    Message = @"HQ restored connection to Export Service",
                //    Type = FatalExceptionType.HqExportServiceUnavailable
                //});

                return result;
            }
            catch (Exception e)
            {
                this.logger.Fatal(@"HQ lost connection to Export Service",
                    e.WithFatalType(FatalExceptionType.HqExportServiceUnavailable));

                return new DataExportStatusView
                {
                    Success = false
                };
            }
        }
    }
}
