using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Storage;
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class BinaryFormatDataExportHandler : AbstractDataExportToZipArchiveHandler
    {
        private readonly IImageFileStorage imageFileRepository;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IInterviewFactory interviewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public BinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IImageFileStorage imageFileRepository,
            IAudioFileStorage audioFileStorage,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IInterviewFactory interviewFactory,
            IDataExportProcessesService dataExportProcessesService,
            IQuestionnaireStorage questionnaireStorage,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.imageFileRepository = imageFileRepository;
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.audioFileStorage = audioFileStorage;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;

        protected override void ExportDataIntoArchive(IZipArchive archive, ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var allMultimediaAnswers = this.interviewFactory.GetMultimediaAnswersByQuestionnaire(settings.QuestionnaireId);

            cancellationToken.ThrowIfCancellationRequested();

            var allAudioAnswers = this.interviewFactory.GetAudioAnswersByQuestionnaire(settings.QuestionnaireId);

            var interviewIds = allMultimediaAnswers.Select(x => x.InterviewId)
                .Union(allAudioAnswers.Select(x => x.InterviewId)).Distinct().ToList();

            cancellationToken.ThrowIfCancellationRequested();

            BlockingCollection<(string path, byte[] content)> filesToZip = new BlockingCollection<(string, byte[])>(50);

            var zipTask = Task.Factory.StartNew(() =>
            {
                foreach (var entry in filesToZip.GetConsumingEnumerable())
                {
                    archive.CreateEntry(entry.path, entry.content);
                }
            }, cancellationToken);

            long totalInterviewsProcessed = 0;
            foreach (var interviewId in interviewIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                foreach (var imageFileName in allMultimediaAnswers.Where(x => x.InterviewId == interviewId).Select(x => x.Answer))
                {
                    var fileContent = imageFileRepository.GetInterviewBinaryData(interviewId, imageFileName);

                    if (fileContent == null) continue;

                    var path = this.fileSystemAccessor.CombinePath(interviewId.FormatGuid(), imageFileName);
                    filesToZip.Add((path, fileContent), cancellationToken);
                }

                foreach (var audioFileName in allAudioAnswers.Where(x => x.InterviewId == interviewId).Select(x => x.Answer))
                {
                    var fileContent = audioFileStorage.GetInterviewBinaryData(interviewId, audioFileName);

                    if (fileContent == null) continue;

                    var path = this.fileSystemAccessor.CombinePath(interviewId.FormatGuid(), audioFileName);
                    filesToZip.Add((path, fileContent), cancellationToken);
                }

                totalInterviewsProcessed++;
                progress.Report(totalInterviewsProcessed.PercentOf(interviewIds.Count));
            }

            filesToZip.CompleteAdding();
            zipTask.Wait(cancellationToken);
        }
    }
}
