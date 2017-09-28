using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class BinaryFormatDataExportHandler : AbstractDataExportHandler
    {
        private readonly IImageFileStorage imageFileRepository;
        private readonly IAudioFileStorage audioFileStorage;

        private readonly ITransactionManager transactionManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly IInterviewFactory interviewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        public BinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IImageFileStorage imageFileRepository,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
            ITransactionManager transactionManager,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            IInterviewFactory interviewFactory,
            IDataExportProcessesService dataExportProcessesService,
            IQuestionnaireStorage questionnaireStorage,
            IDataExportFileAccessor dataExportFileAccessor,
            IAudioFileStorage audioFileStorage,
            IPlainTransactionManagerProvider plainTransactionManagerProvider)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, dataExportFileAccessor)
        {
            this.imageFileRepository = imageFileRepository;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.audioFileStorage = audioFileStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;

        protected override void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            var multimediaQuestionIds = questionnaire.Find<IMultimediaQuestion>().Select(x => x.PublicKey).ToArray();

            cancellationToken.ThrowIfCancellationRequested();
            
            var allMultimediaAnswers = this.transactionManager.ExecuteInQueryTransaction(
                () => this.interviewFactory.GetAllMultimediaAnswers(multimediaQuestionIds));

            cancellationToken.ThrowIfCancellationRequested();

            var allAudioAnswers = this.transactionManager.ExecuteInQueryTransaction(
                () => this.interviewFactory.GetAllAudioAnswers());

            var interviewIds = allMultimediaAnswers.Select(x => x.InterviewId)
                .Union(allAudioAnswers.Select(x => x.InterviewId)).Distinct().ToList();

            cancellationToken.ThrowIfCancellationRequested();

            long totalInterviewsProcessed = 0;
            foreach (var interviewId in interviewIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var interviewDirectory = this.fileSystemAccessor.CombinePath(directoryPath, interviewId.FormatGuid());

                if (!this.fileSystemAccessor.IsDirectoryExists(interviewDirectory))
                    this.fileSystemAccessor.CreateDirectory(interviewDirectory);

                foreach (var imageFileName in allMultimediaAnswers.Where(x=>x.InterviewId == interviewId).Select(x=>x.Answer))
                {
                    var fileContent = imageFileRepository.GetInterviewBinaryData(interviewId, imageFileName);

                    if (fileContent != null)
                    {
                        var pathToFile = this.fileSystemAccessor.CombinePath(interviewDirectory, imageFileName);
                        this.fileSystemAccessor.WriteAllBytes(pathToFile, fileContent);
                    }
                }

                foreach (var audioFileName in allAudioAnswers.Where(x=>x.InterviewId == interviewId).Select(x=>x.Answer))
                {
                    var fileContent = this.plainTransactionManagerProvider.GetPlainTransactionManager().ExecuteInQueryTransaction(
                            () => audioFileStorage.GetInterviewBinaryData(interviewId, audioFileName));

                    if (fileContent != null)
                    {
                        var pathToFile = this.fileSystemAccessor.CombinePath(interviewDirectory, audioFileName);
                        this.fileSystemAccessor.WriteAllBytes(pathToFile, fileContent);
                    }
                }

                totalInterviewsProcessed++;
                progress.Report(totalInterviewsProcessed.PercentOf(interviewIds.Count));
            }
        }
    }
}