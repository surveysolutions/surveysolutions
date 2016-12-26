﻿using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Security;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    public class BinaryFormatDataExportHandler : IExportProcessHandler<DataExportProcessDetails>
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPlainInterviewFileStorage plainFileRepository;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;
        private readonly IZipArchiveProtectionService archiveUtils;
        private readonly ITransactionManager transactionManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IExportSettings exportSettings;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly IDataExportProcessesService dataExportProcessesService;

        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();

        private const string temporaryTabularExportFolder = "TemporaryBinaryExport";
        private readonly string pathToExportedData;

        public BinaryFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor, 
            IPlainInterviewFileStorage plainFileRepository, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            ITransactionManager transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            IZipArchiveProtectionService archiveUtils, 
            IReadSideKeyValueStorage<InterviewData> interviewDatas, 
            IDataExportProcessesService dataExportProcessesService, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IExportSettings exportSettings,
            IPlainTransactionManagerProvider plainTransactionManagerProvider)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.plainFileRepository = plainFileRepository;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.archiveUtils = archiveUtils;
            this.interviewDatas = interviewDatas;
            this.dataExportProcessesService = dataExportProcessesService;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.exportSettings = exportSettings;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;

            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, temporaryTabularExportFolder);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public void ExportData(DataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            List<Guid> interviewIdsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ =>
                        _.Where(x => x.QuestionnaireId == dataExportProcessDetails.Questionnaire.QuestionnaireId &&
                                     x.QuestionnaireVersion == dataExportProcessDetails.Questionnaire.Version && !x.IsDeleted)
                            .OrderBy(x => x.InterviewId)
                            .Select(x => x.InterviewId).ToList()));

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            string folderForDataExport = GetFolderPathOfDataByQuestionnaire(dataExportProcessDetails.Questionnaire);

            this.ClearFolder(folderForDataExport);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            QuestionnaireExportStructure questionnaire =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(dataExportProcessDetails.Questionnaire.QuestionnaireId,
                        dataExportProcessDetails.Questionnaire.Version));
       
            var multimediaQuestionIds =
                questionnaire.HeaderToLevelMap.Values.SelectMany(
                    x => x.HeaderItems.Values.Where(h => h.QuestionType == QuestionType.Multimedia)).Select(x=>x.PublicKey).ToArray();

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            long totalInterviewsProcessed = 0;
            foreach (var interviewId in interviewIdsToExport)
            {
                dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

                var interviewBinaryFiles = plainFileRepository.GetBinaryFilesForInterview(interviewId);
                var filesFolderForInterview = this.fileSystemAccessor.CombinePath(folderForDataExport, interviewId.FormatGuid());

                if (interviewBinaryFiles.Count > 0)
                {
                    var interviewDetails =
                        this.transactionManager.ExecuteInQueryTransaction(() => interviewDatas.GetById(interviewId));
                    if (interviewDetails != null && !interviewDetails.IsDeleted)
                    {
                        var questionsWithAnswersOnMultimediaQuestions = interviewDetails.Levels.Values.SelectMany(
                            level =>
                                level.QuestionsSearchCache.Values.Where(
                                    question =>
                                        question.IsAnswered() && !question.IsDisabled() &&
                                        multimediaQuestionIds.Any(
                                            multimediaQuestionId => question.Id == multimediaQuestionId))
                                    .Select(q => q.Answer.ToString())).ToArray();

                        if (questionsWithAnswersOnMultimediaQuestions.Any())
                        {
                            this.fileSystemAccessor.CreateDirectory(filesFolderForInterview);

                            foreach (
                                var questionsWithAnswersOnMultimediaQuestion in
                                    questionsWithAnswersOnMultimediaQuestions)
                            {
                                var fileContent = plainFileRepository.GetInterviewBinaryData(interviewId,
                                    questionsWithAnswersOnMultimediaQuestion);
                                this.fileSystemAccessor.WriteAllBytes(
                                    this.fileSystemAccessor.CombinePath(filesFolderForInterview,
                                        questionsWithAnswersOnMultimediaQuestion), fileContent);
                            }
                        }
                    }
                }
                totalInterviewsProcessed++;
                this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId,
                    totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count));
            }

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var archiveFilePath =
                this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(dataExportProcessDetails.Questionnaire,
                    DataExportFormat.Binary);
            RecreateExportArchive(folderForDataExport, archiveFilePath);
        }

        private string GetFolderPathOfDataByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData,
                $"{questionnaireIdentity.QuestionnaireId}_{questionnaireIdentity.Version}");
        }

        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
        private void RecreateExportArchive(string folderForDataExport, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }

            var password = this.GetPasswordFromSettings();
            this.archiveUtils.ZipDirectory(folderForDataExport, archiveFilePath, password);
        }

        private string GetPasswordFromSettings()
        {
            return this.PlainTransactionManager.ExecuteInPlainTransaction(() =>
                this.exportSettings.EncryptionEnforced()
                    ? this.exportSettings.GetPassword()
                    : null);
        }
    }
}