using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Storage;
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal interface IBinaryDataSource
    {
        //IEnumerable<(string path, byte[] content)> GetInterviewBinaryData();
        Task ForEachMultimediaAnswerAsync(ExportSettings settings, Func<BinaryData, Task> action, CancellationToken cancellationToken);
    }

    public class BinaryData
    {
        public Guid InterviewId { get; set; }
        public string Answer { get; set; }
        public byte[] Content { get; set; }
    }

    internal class BinaryDataSource : IBinaryDataSource
    {
        private readonly IImageFileStorage imageFileRepository;
        private readonly IInterviewFactory interviewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public BinaryDataSource(
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IInterviewFactory interviewFactory,
            IQuestionnaireStorage questionnaireStorage,
            IImageFileStorage imageFileRepository,
            ITenantApi<IHeadquartersApi> tenantApi,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.tenantApi = tenantApi;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.imageFileRepository = imageFileRepository;
        }

        public async Task ForEachMultimediaAnswerAsync(ExportSettings settings, Func<BinaryData, Task> action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var api = this.tenantApi.For(settings.Tenant);

            var interviewsToExport = await api.GetInterviewsToExportAsync(
                settings.QuestionnaireId,
                settings.InterviewStatus,
                settings.FromDate,
                settings.ToDate
            );

            var questionnaire = await this.questionnaireStorage
                .GetQuestionnaireAsync(settings.Tenant, settings.QuestionnaireId);

            var batchSize = interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery;

            foreach (var interviewBatch in interviewsToExport.Batch(batchSize))
            {
                var interviewIds = interviewBatch.Select(i => i.Id).ToArray();

                var allMultimediaAnswers = this.interviewFactory.GetMultimediaAnswersByQuestionnaire(
                    settings.Tenant, questionnaire, interviewIds, cancellationToken).Result;

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var answer in allMultimediaAnswers)
                {
                    byte[] content;

                    switch (answer.Type)
                    {
                        case MultimediaType.Image:
                            content = imageFileRepository.GetInterviewBinaryData(answer.InterviewId, answer.Answer);
                            break;
                        case MultimediaType.Audio:
                            content = api.GetInterviewAudioAsync(answer.InterviewId, answer.Answer).Result;
                            //audioFileStorage.GetInterviewBinaryData(answer.InterviewId, answer.Answer);
                            break;
                        default:
                            continue;
                    }

                    await action(new BinaryData
                    {
                        InterviewId = answer.InterviewId,
                        Answer = answer.Answer,
                        Content = content
                    });
                }
            }
        }
    }
    
    internal class BinaryFormatDataExportHandler : AbstractDataExportToZipArchiveHandler
    {
        private readonly IBinaryDataSource binaryDataSource;
        
        public BinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor, IBinaryDataSource binaryDataSource)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.binaryDataSource = binaryDataSource;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;

        protected override void ExportDataIntoArchive(IZipArchive archive, ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            binaryDataSource.ForEachMultimediaAnswerAsync(settings,  data =>
            {
                var path = this.fileSystemAccessor.CombinePath(data.InterviewId.FormatGuid(), data.Answer);
                archive.CreateEntry(path, data.Content);
                return Task.CompletedTask;
            }, cancellationToken).Wait();
        }
    }
}
