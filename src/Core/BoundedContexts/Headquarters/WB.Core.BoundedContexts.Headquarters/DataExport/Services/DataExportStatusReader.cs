#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native;
using WB.ServicesIntegration.Export;
using InterviewStatus = WB.Core.SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus;
using QuestionnaireIdentity = WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionnaireIdentity;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly IExportServiceApi exportServiceApi;
        private readonly IExportFileNameService exportFileNameService;
        private readonly IQuestionnaireStorage questionnaireStorage;

        // TODO: Restore SLACK notifications
        //private readonly ISlackApiClient slackApiClient;
        private readonly ILogger logger;
        private readonly IAudioAuditFileStorage audioAuditFileStorage;

        public DataExportStatusReader(
            IExportServiceApi exportServiceApi,
            IExportFileNameService exportFileNameService,
            IQuestionnaireStorage questionnaireStorage,
            IAudioAuditFileStorage audioAuditFileStorage,
            // TODO: Restore SLACK notifications
            // ISlackApiClient slackApiClient,
            ILoggerProvider loggerProvider)
        {
            this.exportServiceApi = exportServiceApi;
            this.exportFileNameService = exportFileNameService;
            this.questionnaireStorage = questionnaireStorage;
            // TODO: Restore SLACK notifications
            //    this.slackApiClient = slackApiClient;
            this.logger = loggerProvider.GetForType(this.GetType());
            this.audioAuditFileStorage = audioAuditFileStorage;
        }

        public async Task<DataExportArchive?> GetDataArchive(long jobId)
        {
            var jobInfo = await exportServiceApi.GetJobsStatus(jobId);
            var archiveFileName = exportFileNameService.GetQuestionnaireTitleWithVersion(
                new QuestionnaireIdentity(jobInfo.QuestionnaireIdentity.Id, jobInfo.QuestionnaireIdentity.Version)
            );
            
            var result = await exportServiceApi.DownloadArchive(jobId, archiveFileName);
            
            if (result.StatusCode == HttpStatusCode.NotFound) return null;

            result.EnsureSuccessStatusCode();

            if (result.Headers.TryGetValues(@"NewLocation", out var values))
            {
                return new DataExportArchive
                {
                    Redirect = values.First()
                };
            }

            if (result.Content.Headers.ContentDisposition == null) return null;

            return new DataExportArchive
            {
                FileName = result.Content.Headers.ContentDisposition.FileName,
                Data = await result.Content.ReadAsStreamAsync()
            };
        }

        public async Task<ExportDataAvailabilityView?> GetDataAvailabilityAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            if (questionnaire == null)
                return null;

            var hasAudioAuditFiles = await this.audioAuditFileStorage.HasAnyAudioAuditFilesStoredAsync(questionnaireIdentity);
            return new ExportDataAvailabilityView
            {
                HasBinaryData = questionnaire.HasAnyMultimediaQuestion() 
                || hasAudioAuditFiles,
                HasInterviews = true
            };
        }

        public async Task<DataExportProcessView?> GetProcessStatus(long id)
        {
            DataExportProcessView processView = await this.exportServiceApi.GetJobsStatus(id);
            if (processView == null)
            {
                return null;
            }
            
            FillProcessViewMissingData(processView);

            processView.Id = id;
            return processView;
        }

        private void FillProcessViewMissingData(DataExportProcessView processView)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(
                new QuestionnaireIdentity(processView.QuestionnaireIdentity!.Id,
                    processView.QuestionnaireIdentity.Version),
                null);
            if (questionnaire == null)
            {
                processView.Deleted = true;
                return;
            };

            processView.Title = questionnaire.Title;
            if (processView.TranslationId.HasValue)
            {
                var translation = questionnaire.Translations.FirstOrDefault(x =>x.Id == processView.TranslationId);
                processView.TranslationName = translation?.Name;
            }
            else
            {
                processView.TranslationName = questionnaire.DefaultLanguageName;
            }
            
            if (processView.Error != null)
            {
                processView.Error.Message = processView.Error.Type switch
                {
                    DataExportError.Canceled => Resources.DataExport.Error_Canceled,
                    DataExportError.NotEnoughExternalStorageSpace => Resources.DataExport.Error_NotEnoughExternalStorageSpace,
                    _ => Resources.DataExport.Error_Unhandled
                };
            }
        }

        public async Task<List<DataExportProcessView>> GetProcessStatuses(long[] ids)
        {
            var processViews = await this.exportServiceApi.GetJobsStatuses(ids);

            foreach (var processView in processViews.Where(p => p != null))
            {
                FillProcessViewMissingData(processView);
            }

            return processViews;
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
