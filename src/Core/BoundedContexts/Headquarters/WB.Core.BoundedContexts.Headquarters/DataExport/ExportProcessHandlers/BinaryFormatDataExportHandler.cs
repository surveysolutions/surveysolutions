using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
        
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;
        private readonly ITransactionManager transactionManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly ILogger logger;

        public BinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor, 
            IImageFileStorage imageFileRepository, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            ITransactionManager transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            IReadSideKeyValueStorage<InterviewData> interviewDatas, 
            IDataExportProcessesService dataExportProcessesService, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IDataExportFileAccessor dataExportFileAccessor,
            IAudioFileStorage audioFileStorage,
            IPlainTransactionManagerProvider plainTransactionManagerProvider,
            ILogger logger)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor)
        {
            this.imageFileRepository = imageFileRepository;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.interviewDatas = interviewDatas;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.audioFileStorage = audioFileStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.logger = logger;
        }

        protected override DataExportFormat Format => DataExportFormat.Binary;

        protected override void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            List<Guid> interviewIdsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ =>
                        _.Where(x => x.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                                     x.QuestionnaireVersion == questionnaireIdentity.Version)
                            .OrderBy(x => x.InterviewId)
                            .Select(x => x.InterviewId).ToList()));

            cancellationToken.ThrowIfCancellationRequested();

            QuestionnaireExportStructure questionnaire = this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(questionnaireIdentity);

            var multimediaQuestionIds = questionnaire.HeaderToLevelMap.Values.SelectMany(
                    x => x.HeaderItems.Values.OfType<ExportedHeaderItem>().Where(h => h.QuestionType == QuestionType.Multimedia)).Select(x => x.PublicKey).ToHashSet();

            var audioQuestionIds = questionnaire.HeaderToLevelMap.Values.SelectMany(
                    x => x.HeaderItems.Values.OfType<ExportedHeaderItem>().Where(h => h.QuestionType == QuestionType.Audio)).Select(x => x.PublicKey).ToHashSet();
            
            long totalInterviewsProcessed = 0;
            foreach (var interviewId in interviewIdsToExport)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var filesFolderForInterview = this.fileSystemAccessor.CombinePath(directoryPath, interviewId.FormatGuid());

                var interviewDetails = this.transactionManager.ExecuteInQueryTransaction(() => interviewDatas.GetById(interviewId));
                if (interviewDetails != null)
                {
                    var questionsWithAnswersOnMultimediaQuestions = interviewDetails.Levels.Values.SelectMany(
                        level => level.QuestionsSearchCache.Values.Where(
                                question => question.IsAnswered() && !question.IsDisabled() &&
                                            multimediaQuestionIds.Contains(question.Id))).ToArray();

                    if (questionsWithAnswersOnMultimediaQuestions.Any())
                    {
                        if (!this.fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                            this.fileSystemAccessor.CreateDirectory(filesFolderForInterview);

                        foreach (var answeredImageQuestion in questionsWithAnswersOnMultimediaQuestions)
                        {
                            var imageFileName = answeredImageQuestion.Answer.ToString();

                            var fileContent = imageFileRepository.GetInterviewBinaryData(interviewId, imageFileName);

                            if (fileContent == null)
                                this.logger.Error($"Binary export. Image {imageFileName} not found. Interview [{interviewId}]. Question [{answeredImageQuestion.Id}]");
                            else
                            {
                                var pathToFile = this.fileSystemAccessor.CombinePath(filesFolderForInterview, imageFileName);
                                this.fileSystemAccessor.WriteAllBytes(pathToFile, fileContent);
                            }
                        }
                    }

                    var questionsWithAnswersOnAudioQuestions = interviewDetails.Levels.Values.SelectMany(
                        level =>
                            level.QuestionsSearchCache.Values.Where(
                                    question =>
                                        question.IsAnswered() && !question.IsDisabled() &&
                                        audioQuestionIds.Contains(question.Id))
                                .Select(q => q.Answer.ToString())).ToArray();

                    if (questionsWithAnswersOnAudioQuestions.Any())
                    {
                        if (!this.fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                            this.fileSystemAccessor.CreateDirectory(filesFolderForInterview);

                        foreach (var questionsWithAnswersOnAudioQuestion in questionsWithAnswersOnAudioQuestions)
                        {
                            var manager = this.plainTransactionManagerProvider.GetPlainTransactionManager();
                            var fileContent = manager.ExecuteInQueryTransaction(
                                () => audioFileStorage.GetInterviewBinaryData(interviewId, questionsWithAnswersOnAudioQuestion));

                            if (fileContent != null)
                            {
                                var pathToFile = this.fileSystemAccessor.CombinePath(filesFolderForInterview, questionsWithAnswersOnAudioQuestion);
                                this.fileSystemAccessor.WriteAllBytes(pathToFile, fileContent);
                            }
                        }
                    }
                }

                totalInterviewsProcessed++;
                progress.Report(totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count));
            }
        }
    }
}