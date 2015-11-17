﻿using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    public class BinaryFormatDataExportHandler : IExportProcessHandler<AllDataExportDetails>
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPlainInterviewFileStorage plainFileRepository;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;
        private readonly IArchiveUtils archiveUtils;
        private readonly ITransactionManager transactionManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader;
        private readonly IDataExportProcessesService dataExportProcessesService;

        private const string temporaryTabularExportFolder = "TemporaryBinaryExport";
        private readonly string pathToExportedData;

        public BinaryFormatDataExportHandler(IFileSystemAccessor fileSystemAccessor, IPlainInterviewFileStorage plainFileRepository, IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings, ITransactionManager transactionManager, IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, IArchiveUtils archiveUtils, IReadSideKeyValueStorage<InterviewData> interviewDatas, IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader, IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.plainFileRepository = plainFileRepository;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.archiveUtils = archiveUtils;
            this.interviewDatas = interviewDatas;
            this.questionnaireReader = questionnaireReader;
            this.dataExportProcessesService = dataExportProcessesService;

            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, temporaryTabularExportFolder);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public void ExportData(AllDataExportDetails dataExportDetails)
        {
            List<Guid> interviewIdsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ =>
                        _.Where(x => x.QuestionnaireId == dataExportDetails.Questionnaire.QuestionnaireId &&
                                     x.QuestionnaireVersion == dataExportDetails.Questionnaire.Version && !x.IsDeleted)
                            .OrderBy(x => x.InterviewId)
                            .Select(x => x.InterviewId).ToList()));

            string folderForDataExport = GetFolderPathOfDataByQuestionnaire(dataExportDetails.Questionnaire);

            this.ClearFolder(folderForDataExport);

            QuestionnaireExportStructure questionnaire =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.questionnaireReader.AsVersioned()
                        .Get(dataExportDetails.Questionnaire.QuestionnaireId.FormatGuid(), dataExportDetails.Questionnaire.Version));

            var multimediaQuestionIds =
                questionnaire.HeaderToLevelMap.Values.SelectMany(
                    x => x.HeaderItems.Values.Where(h => h.QuestionType == QuestionType.Multimedia)).Select(x=>x.PublicKey).ToArray();

            int totalInterviewsProcessed = 0;
            foreach (var interviewId in interviewIdsToExport)
            {
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
                this.dataExportProcessesService.UpdateDataExportProgress(dataExportDetails.ProcessId,
                    totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count));
            }

            var archiveFilePath =
                this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(dataExportDetails.Questionnaire,
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
            this.archiveUtils.ZipDirectory(folderForDataExport, archiveFilePath);
        }
    }
}