using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Storage;
using WB.Services.Export.Utils;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal abstract class AbstractExternalStorageDataExportHandler : AbstractDataExportHandler,
        IExportProcessHandler<ExportBinaryToExternalStorage>
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        
        private readonly IInterviewFactory interviewFactory;
        private readonly IImageFileStorage imageFileRepository;
        private readonly IAudioFileStorage audioFileStorage;
        

        protected AbstractExternalStorageDataExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService, 
            IDataExportFileAccessor dataExportFileAccessor,
            IQuestionnaireStorage questionnaireStorage,
            IInterviewFactory interviewFactory,
            IImageFileStorage imageFileRepository,
            IAudioFileStorage audioFileStorage) :
            base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.interviewFactory = interviewFactory;
            this.imageFileRepository = imageFileRepository;
            this.audioFileStorage = audioFileStorage;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;
        protected override bool CompressExportedData => false;
        
        protected override void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaireAsync(settings.Tenant, settings.QuestionnaireId).Result;

            cancellationToken.ThrowIfCancellationRequested();

            var allMultimediaAnswers = this.interviewFactory.GetMultimediaAnswersByQuestionnaire(settings.QuestionnaireId);

            cancellationToken.ThrowIfCancellationRequested();

            var allAudioAnswers = this.interviewFactory.GetAudioAnswersByQuestionnaire(settings.QuestionnaireId);

            var interviewIds = allMultimediaAnswers.Select(x => x.InterviewId)
                .Union(allAudioAnswers.Select(x => x.InterviewId)).Distinct().ToList();

            cancellationToken.ThrowIfCancellationRequested();

            long totalInterviewsProcessed = 0;

            if (!interviewIds.Any()) return;

            using (this.GetClient(this.accessToken))
            {
                var applicationFolder = this.CreateApplicationFolder();

                string GetInterviewFolder(Guid interviewId) => $"{settings.ArchiveName})/{interviewId.FormatGuid()}";
               
                foreach (var interviewId in interviewIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var interviewFolderPath = this.CreateFolder(applicationFolder, GetInterviewFolder(interviewId));

                    foreach (var imageFileName in allMultimediaAnswers.Where(x => x.InterviewId == interviewId)
                        .Select(x => x.Answer))
                    {
                        var fileContent = imageFileRepository.GetInterviewBinaryData(interviewId, imageFileName);

                        if (fileContent != null)
                            this.UploadFile(interviewFolderPath, fileContent, imageFileName);
                    }

                    foreach (var audioFileName in allAudioAnswers.Where(x => x.InterviewId == interviewId)
                        .Select(x => x.Answer))
                    {
                        var fileContent = audioFileStorage.GetInterviewBinaryData(interviewId, audioFileName);

                        if (fileContent != null)
                            this.UploadFile(interviewFolderPath, fileContent, audioFileName);
                    }

                    totalInterviewsProcessed++;
                    progress.Report(totalInterviewsProcessed.PercentOf(interviewIds.Count));
                }
            }
        }

        public void ExportData(ExportBinaryToExternalStorage process)
        {
            this.accessToken = process.AccessToken;
            base.ExportData(process);
        }

        protected abstract IDisposable GetClient(string accessToken);
        private string accessToken;

        protected abstract string CreateApplicationFolder();
        protected abstract string CreateFolder(string applicationFolder, string folderName);

        protected abstract void UploadFile(string folder, byte[] fileContent, string fileName);
    }
}
