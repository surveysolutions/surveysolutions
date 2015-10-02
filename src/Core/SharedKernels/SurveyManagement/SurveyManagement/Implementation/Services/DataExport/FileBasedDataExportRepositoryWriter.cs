using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Resources;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class FileBasedDataExportRepositoryWriter : IDataExportRepositoryWriter
    {
        private readonly IDataExportWriter dataExportWriter;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;

        private readonly ILogger logger;

        private readonly IExportViewFactory exportViewFactory;

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPlainInterviewFileStorage plainFileRepository;

        private readonly IReadSideKeyValueStorage<InterviewData> interviewDataWriter;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter;

        private readonly InterviewExportedAction[] interviewActionsForDataUpdate = new[]
        {
            InterviewExportedAction.ApprovedByHeadquarter, InterviewExportedAction.SupervisorAssigned,
            InterviewExportedAction.Completed, InterviewExportedAction.ApprovedBySupervisor,
            InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restored,
            InterviewExportedAction.UnapprovedByHeadquarter
        };

        public FileBasedDataExportRepositoryWriter(
            IDataExportWriter dataExportWriter,
            IEnvironmentContentService environmentContentService,
            IFileSystemAccessor fileSystemAccessor, 
            ILogger logger, 
            IPlainInterviewFileStorage plainFileRepository,
            IReadSideKeyValueStorage<InterviewData> interviewDataWriter,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter,
            IExportViewFactory exportViewFactory, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor)
        {
            this.dataExportWriter = dataExportWriter;
            this.environmentContentService = environmentContentService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.plainFileRepository = plainFileRepository;
            this.interviewDataWriter = interviewDataWriter;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.interviewSummaryWriter = interviewSummaryWriter;
            this.exportViewFactory = exportViewFactory;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }

        public void Clear()
        {
            this.dataExportWriter.Clear();
            this.filebasedExportedDataAccessor.CleanExportDataFolder();
            this.filebasedExportedDataAccessor.CleanExportFileFolder();
        }

        public void CreateExportStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure)
        {
            this.CreateExportedDataStructure(questionnaireExportStructure);
            this.CreateExportedFileStructure(questionnaireExportStructure);
        }

        public void AddOrUpdateExportedDataByInterviewWithAction(Guid interviewId, InterviewExportedAction action)
        {
            var interviewSummary = interviewSummaryWriter.GetById(interviewId);
            if (interviewSummary == null)
                return;

            filebasedExportedDataAccessor.DeleteAllDataFolder(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            if (action == InterviewExportedAction.ApprovedByHeadquarter || action == InterviewExportedAction.UnapprovedByHeadquarter)
                filebasedExportedDataAccessor.DeleteApprovedDataFolder(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            if (interviewActionsForDataUpdate.Contains(action))
            {
                var interviewDataExportView = this.CreateInterviewDataExportView(interviewId, action);

                if (interviewDataExportView == null)
                    return;

                this.AddExportedDataByInterviewImpl(interviewDataExportView);
            }
        }

        public void DeleteInterview(Guid interviewId)
        {
            var interviewSummary = interviewSummaryWriter.GetById(interviewId);
            if (interviewSummary == null)
                return;

            this.DeleteInterviewImpl(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion,
                interviewId);
        }

        public void DeleteExportedDataForQuestionnaireVersion(Guid questionnaireId, long questionnaireVersion)
        {
            var dataFolderForTemplatePath =
                this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId,
                    questionnaireVersion);

            this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);

            var filesFolderForTemplatePath =
                this.filebasedExportedDataAccessor.GetFolderPathOfFilesByQuestionnaire(questionnaireId,
                    questionnaireVersion);

            this.fileSystemAccessor.DeleteDirectory(filesFolderForTemplatePath);
        }

        private void CreateExportedDataStructure(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var dataFolderForTemplatePath =
                this.filebasedExportedDataAccessor.CreateExportDataFolder(questionnaireExportStructure.QuestionnaireId,
                    questionnaireExportStructure.Version);

            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                this.environmentContentService.CreateContentOfAdditionalFile(headerStructureForLevel,
                    ExportFileSettings.GetContentFileName(headerStructureForLevel.LevelName), dataFolderForTemplatePath);
            }
        }

        private void CreateExportedFileStructure(QuestionnaireExportStructure questionnaireExportStructure)
        {
            this.filebasedExportedDataAccessor.CreateExportFileFolder(questionnaireExportStructure.QuestionnaireId,
                questionnaireExportStructure.Version);
        }

        private void DeleteInterviewImpl(Guid questionnaireId, long questionnaireVersion, Guid interviewId)
        {
            this.dataExportWriter.DeleteInterviewRecords(interviewId);

            var filesFolderForInterview =
                this.filebasedExportedDataAccessor.GetFolderPathOfFilesByQuestionnaireForInterview(questionnaireId,
                    questionnaireVersion, interviewId);

            if (fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                fileSystemAccessor.DeleteDirectory(filesFolderForInterview);

            filebasedExportedDataAccessor.DeleteAllDataFolder(questionnaireId,
                    questionnaireVersion);
        }

        private void AddExportedDataByInterviewImpl(InterviewDataExportView interviewDataExportView)
        {
            this.dataExportWriter.AddOrUpdateInterviewRecords(interviewDataExportView,
                interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);
             
            var filesFolderForInterview =
                this.filebasedExportedDataAccessor.GetFolderPathOfFilesByQuestionnaireForInterview(
                    interviewDataExportView.TemplateId,
                    interviewDataExportView.TemplateVersion, interviewDataExportView.InterviewId);

            if (fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                fileSystemAccessor.DeleteDirectory(filesFolderForInterview);

            var questionsWithAnswersOnMultimediaQuestions = this.GetAllMultimediaQuestionFileNames(interviewDataExportView);

            if (questionsWithAnswersOnMultimediaQuestions.Any())
            {
                if (fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                    fileSystemAccessor.CreateDirectory(filesFolderForInterview);
            }

            foreach (var questionWithAnswersOnMultimediaQuestions in questionsWithAnswersOnMultimediaQuestions)
            {
                var fileContent = plainFileRepository.GetInterviewBinaryData(interviewDataExportView.InterviewId,
                    questionWithAnswersOnMultimediaQuestions);

                if (fileContent == null || fileContent.Length == 0)
                {
                    logger.Error(
                        string.Format(
                            FileBasedDataExportRepositoryWriterMessages
                                .FileContentIsMissingForFileNameAndInterviewFormat,
                            questionWithAnswersOnMultimediaQuestions, interviewDataExportView.InterviewId));
                    continue;
                }
                
                this.fileSystemAccessor.WriteAllBytes(
                    this.fileSystemAccessor.CombinePath(filesFolderForInterview,
                        questionWithAnswersOnMultimediaQuestions), fileContent);
            }
        }

        private string[] GetAllMultimediaQuestionFileNames(InterviewDataExportView interviewDataExportView)
        {
            var questionsWithAnswersOnMultimediaQuestions = interviewDataExportView.Levels.SelectMany(
                level =>
                    level.Records.SelectMany(
                        record =>
                            record.Questions.Where(
                                question =>
                                    question.QuestionType == QuestionType.Multimedia && question.Answers != null &&
                                    question.Answers.Length > 0 && !string.IsNullOrEmpty(question.Answers[0]))));

            return questionsWithAnswersOnMultimediaQuestions.Select(a => a.Answers[0]).ToArray();
        }

        private InterviewDataExportView CreateInterviewDataExportView(Guid interviewId, InterviewExportedAction action,
            QuestionnaireExportStructure questionnaireExportStructure = null)
        {
            var interview = interviewDataWriter.GetById(interviewId);
            if (interview == null || interview.IsDeleted)
                return null;

            if (questionnaireExportStructure == null)
            {
                questionnaireExportStructure =
                    questionnaireExportStructureWriter.AsVersioned()
                        .Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);
                if (questionnaireExportStructure == null)
                    return null;
            }
            return exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure, interview, action);
        }

        public void EnableCache()
        {
            dataExportWriter.EnableCache();
        }

        public void DisableCache()
        {
            dataExportWriter.DisableCache();
        }

        public bool IsCacheEnabled
        {
            get { return dataExportWriter.IsCacheEnabled; }
        }

        public string GetReadableStatus()
        {
            return dataExportWriter.GetReadableStatus();
        }

        public Type ViewType
        {
            get { return dataExportWriter.GetType(); }
        }
    }

}
