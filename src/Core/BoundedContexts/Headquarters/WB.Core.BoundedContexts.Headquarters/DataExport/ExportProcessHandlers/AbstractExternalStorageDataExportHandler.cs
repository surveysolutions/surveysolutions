using System;
using System.Linq;
using System.Threading;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal abstract class AbstractExternalStorageDataExportHandler : AbstractDataExportHandler,
        IExportProcessHandler<ExportBinaryToExternalStorage>
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ITransactionManager transactionManager;
        private readonly IInterviewFactory interviewFactory;
        private readonly IImageFileStorage imageFileRepository;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        protected AbstractExternalStorageDataExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService, 
            IDataExportFileAccessor dataExportFileAccessor,
            IQuestionnaireStorage questionnaireStorage,
            ITransactionManager transactionManager,
            IInterviewFactory interviewFactory,
            IImageFileStorage imageFileRepository,
            IAudioFileStorage audioFileStorage,
            IPlainTransactionManagerProvider plainTransactionManagerProvider) :
            base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.transactionManager = transactionManager;
            this.interviewFactory = interviewFactory;
            this.imageFileRepository = imageFileRepository;
            this.audioFileStorage = audioFileStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;

        protected override void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(settings.QuestionnaireId);
            var multimediaQuestionIds = questionnaire.Find<IMultimediaQuestion>().Select(x => x.PublicKey).ToArray();

            cancellationToken.ThrowIfCancellationRequested();

            var allMultimediaAnswers = this.transactionManager.ExecuteInQueryTransaction(
                () => this.interviewFactory.GetMultimediaAnswersByQuestionnaire(settings.QuestionnaireId,
                    multimediaQuestionIds));

            cancellationToken.ThrowIfCancellationRequested();

            var allAudioAnswers = this.transactionManager.ExecuteInQueryTransaction(
                () => this.interviewFactory.GetAudioAnswersByQuestionnaire(settings.QuestionnaireId));

            var interviewIds = allMultimediaAnswers.Select(x => x.InterviewId)
                .Union(allAudioAnswers.Select(x => x.InterviewId)).Distinct().ToList();

            cancellationToken.ThrowIfCancellationRequested();

            this.ConnectToExternalStorage(this.AccessToken);

            long totalInterviewsProcessed = 0;
            foreach (var interviewId in interviewIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var imageFileName in allMultimediaAnswers.Where(x => x.InterviewId == interviewId)
                    .Select(x => x.Answer))
                {
                    var fileContent = imageFileRepository.GetInterviewBinaryData(interviewId, imageFileName);

                    if (fileContent != null)
                        this.SendImage(interviewId, fileContent, imageFileName);
                }

                foreach (var audioFileName in allAudioAnswers.Where(x => x.InterviewId == interviewId)
                    .Select(x => x.Answer))
                {
                    var fileContent = this.plainTransactionManagerProvider.GetPlainTransactionManager()
                        .ExecuteInQueryTransaction(
                            () => audioFileStorage.GetInterviewBinaryData(interviewId, audioFileName));

                    if (fileContent != null)
                        this.SendAudio(interviewId, fileContent, audioFileName);
                }

                totalInterviewsProcessed++;
                progress.Report(totalInterviewsProcessed.PercentOf(interviewIds.Count));
            }
        }

        public void ExportData(ExportBinaryToExternalStorage process)
        {
            this.AccessToken = process.AccessToken;
            base.ExportData(process);
        }

        public string AccessToken { get; private set; }

        protected abstract void ConnectToExternalStorage(string accessToken);
        protected abstract void SendImage(Guid interviewId, byte[] fileContent, string fileName);
        protected abstract void SendAudio(Guid interviewId, byte[] fileContent, string fileName);
    }
}
