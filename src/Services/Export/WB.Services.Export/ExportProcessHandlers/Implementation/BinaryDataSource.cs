using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class BinaryDataSource : IBinaryDataSource
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly IInterviewsToExportSource interviewsToExportSource;
        private readonly ILogger logger;
        private readonly IOptions<ExportServiceSettings> interviewDataExportSettings;

        public BinaryDataSource(
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IInterviewFactory interviewFactory,
            IQuestionnaireStorage questionnaireStorage,
            ITenantApi<IHeadquartersApi> tenantApi,
            IInterviewsToExportSource interviewsToExportSource,
            ILogger<BinaryDataSource> logger)
        {
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.tenantApi = tenantApi;
            this.interviewsToExportSource = interviewsToExportSource;
            this.logger = logger;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }

        public async Task ForEachInterviewMultimediaAsync(ExportState state, 
            Func<BinaryData, Task> binaryDataAction, CancellationToken cancellationToken)
        {
            var settings = state.Settings;
            var progress = state.Progress;

            cancellationToken.ThrowIfCancellationRequested();
            var api = this.tenantApi.For(settings.Tenant);
            var interviewsToExport = this.interviewsToExportSource.GetInterviewsToExport(
                settings.QuestionnaireId, settings.Status, settings.FromDate, settings.ToDate);

            var questionnaire = await this.questionnaireStorage
                .GetQuestionnaireAsync(settings.QuestionnaireId, token: cancellationToken);

            if(questionnaire == null)
                throw new InvalidOperationException("questionnaire must be not null.");

            var batchSize = interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery;

            progress.Report(0);
            long interviewsProcessed = 0;
            foreach (var interviewBatch in interviewsToExport.Batch(batchSize))
            {
                //var interviewIds = interviewBatch.Select(i => i.Id).ToArray();
                var interviewsKeyMap = interviewBatch.ToDictionary(i => i.Id, i => i.Key);
                var interviewIds = interviewsKeyMap.Keys.ToArray();

                var allMultimediaAnswers = this.interviewFactory.GetMultimediaAnswersByQuestionnaire(questionnaire, interviewIds, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var audioAuditInfos = await this.interviewFactory.GetAudioAuditInfos(settings.Tenant, interviewIds, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                double totalFiles = allMultimediaAnswers.Count + audioAuditInfos.Sum(ai => ai.FileNames.Length);
                long filesUploaded = 0;
                var interviewProgress = interviewsProcessed.PercentOf(interviewsToExport.Count);

                foreach (var answer in allMultimediaAnswers)
                {
                    try
                    {
                        var data = new BinaryData
                        {
                            InterviewId = answer.InterviewId,
                            InterviewKey = interviewsKeyMap[answer.InterviewId],
                            FileName = answer.Answer
                        };

                        switch (answer.Type)
                        {
                            case MultimediaType.Image:
                                var imageContent = await api.GetInterviewImageAsync(answer.InterviewId, answer.Answer);
                                data.Content = imageContent;
                                data.ContentLength = imageContent.Length;
                                data.Type = BinaryDataType.Image;

                                break;
                            case MultimediaType.Audio:
                                var audioContent = await api.GetInterviewAudioAsync(answer.InterviewId, answer.Answer);
                                data.Content = audioContent;
                                data.ContentLength = audioContent.Length;
                                data.Type = BinaryDataType.Audio;
                                
                                break;
                            default:
                                continue;
                        }
                        
                        await binaryDataAction(data);

                        filesUploaded++;

                        var filesPercent = filesUploaded / totalFiles;
                        var batchProgress = (long) (interviewIds.Length * filesPercent );
                        var batchInterviewsProgress = batchProgress.PercentOf(interviewsToExport.Count);
                        
                        progress.Report(interviewProgress + batchInterviewsProgress);
                    }
                    catch(ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        logger.LogWarning("[{statusCode}] Cannot download file for {interviewId} - {answer}", e.StatusCode, answer.InterviewId, answer.Answer);
                    }
                }

                foreach (var audioAuditInfo in audioAuditInfos)
                {
                    foreach (var fileName in audioAuditInfo.FileNames)
                    {
                        try
                        {
                            var audioContent = await api.GetAudioAuditAsync(audioAuditInfo.InterviewId, fileName);

                            await binaryDataAction(new BinaryData
                            {
                                InterviewId = audioAuditInfo.InterviewId,
                                InterviewKey = interviewsKeyMap[audioAuditInfo.InterviewId],
                                FileName = fileName,
                                Content = audioContent,
                                ContentLength = audioContent.Length,
                                Type = BinaryDataType.AudioAudit
                            });

                            filesUploaded++;

                            var filesPercent = filesUploaded / totalFiles;
                            var batchProgress = (long)(interviewIds.Length * filesPercent);
                            var batchInterviewsProgress = batchProgress.PercentOf(interviewsToExport.Count);

                            progress.Report(interviewProgress + batchInterviewsProgress);
                        }
                        catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
                        {
                            logger.LogWarning("[{statusCode}] Cannot download audio audit record for interview: {interviewId} - {fileName}", e.StatusCode, audioAuditInfo.InterviewId, fileName);
                        }
                    }
                }

                interviewsProcessed += interviewIds.Length;
                progress.Report(interviewsProcessed.PercentOf(interviewsToExport.Count));
            }
        }
    }
}
