﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        protected override bool CompressExportedData => false;

        protected override void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(settings.QuestionnaireId);
            var multimediaQuestionIds = questionnaire.Find<IMultimediaQuestion>().Select(x => x.PublicKey).ToArray();

            cancellationToken.ThrowIfCancellationRequested();

            var allMultimediaAnswers = this.transactionManager.ExecuteInQueryTransaction(
                () => this.interviewFactory.GetMultimediaAnswersByQuestionnaire(settings.QuestionnaireId));

            cancellationToken.ThrowIfCancellationRequested();

            var allAudioAnswers = this.transactionManager.ExecuteInQueryTransaction(
                () => this.interviewFactory.GetAudioAnswersByQuestionnaire(settings.QuestionnaireId));

            var interviewIds = allMultimediaAnswers.Select(x => x.InterviewId)
                .Union(allAudioAnswers.Select(x => x.InterviewId)).Distinct().ToList();

            cancellationToken.ThrowIfCancellationRequested();

            long totalInterviewsProcessed = 0;

            if (!interviewIds.Any()) return;

            using (this.GetClient(this.accessToken))
            {
                var applicationFolder = this.CreateApplicationFolder();

                foreach (var interviewId in interviewIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var interviewFolderPath = this.CreateFolder(applicationFolder, interviewId.ToString());

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
                        var fileContent = this.plainTransactionManagerProvider.GetPlainTransactionManager()
                            .ExecuteInQueryTransaction(
                                () => audioFileStorage.GetInterviewBinaryData(interviewId, audioFileName));

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
        protected abstract string CreateFolder(string applicatioFolder, string folderName);

        protected abstract void UploadFile(string folder, byte[] fileContent, string fileName);
    }
}
