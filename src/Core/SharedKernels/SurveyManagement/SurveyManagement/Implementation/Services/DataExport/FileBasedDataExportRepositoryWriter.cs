using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Resources;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
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
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IExportedDataAccessor exportedDataAccessor;

        public FileBasedDataExportRepositoryWriter(
            IDataExportWriter dataExportWriter,
            IEnvironmentContentService environmentContentService,
            IFileSystemAccessor fileSystemAccessor, 
            ILogger logger, 
            IPlainInterviewFileStorage plainFileRepository,
            IReadSideKeyValueStorage<InterviewData> interviewDataWriter,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            IReadSideRepositoryWriter<UserDocument> users,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter,
            IExportViewFactory exportViewFactory, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, IExportedDataAccessor exportedDataAccessor)
        {
            this.dataExportWriter = dataExportWriter;
            this.environmentContentService = environmentContentService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.plainFileRepository = plainFileRepository;
            this.interviewDataWriter = interviewDataWriter;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.users = users;
            this.interviewSummaryWriter = interviewSummaryWriter;
            this.exportViewFactory = exportViewFactory;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.exportedDataAccessor = exportedDataAccessor;
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

        public void AddExportedDataByInterview(Guid interviewId)
        {
            var interviewDataExportView = this.CreateInterviewDataExportView(interviewId);

            if (interviewDataExportView == null)
                return;

            this.AddExportedDataByInterviewImpl(interviewDataExportView);
        }

        public void AddInterviewAction(InterviewExportedAction action, Guid interviewId, Guid userId, DateTime timestamp)
        {
            var interviewSummary = interviewSummaryWriter.GetById(interviewId);

            if (interviewSummary == null || interviewSummary.IsDeleted)
                return;

            this.AddInterviewActionImpl(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion,
                CreateInterviewAction(action, interviewId, userId, timestamp));
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

            var dataFolderForQuestionnaire =
                this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId,
                    questionnaireVersion);

            fileSystemAccessor.DeleteDirectory(exportedDataAccessor.GetAllDataFolder(dataFolderForQuestionnaire));
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

            fileSystemAccessor.CreateDirectory(filesFolderForInterview);

            var questionsWithAnswersOnMultimediaQuestions =
                this.GetAllMultimediaQuestionFileNames(interviewDataExportView);

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

        private void AddInterviewActionImpl(Guid questionnaireId, long questionnaireVersion,
            InterviewActionExportView action)
        {
            this.dataExportWriter.AddActionRecord(action, questionnaireId, questionnaireVersion);

            var dataFolderForQuestionnaire =
                this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId,
                    questionnaireVersion);

            fileSystemAccessor.DeleteDirectory(exportedDataAccessor.GetAllDataFolder(dataFolderForQuestionnaire));

            if (action.Action == InterviewExportedAction.ApproveByHeadquarter)
                fileSystemAccessor.DeleteDirectory(exportedDataAccessor.GetApprovedDataFolder(dataFolderForQuestionnaire));
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

        private InterviewActionExportView CreateInterviewAction(InterviewExportedAction action, Guid interviewId,
            Guid userId, DateTime timestamp)
        {
            UserDocument responsible = this.users.GetById(userId);

            var userName = this.GetUserName(responsible);

            return
                new InterviewActionExportView(interviewId.FormatGuid(), action, userName, timestamp,
                    this.GetUserRole(responsible));
        }

        private InterviewDataExportView CreateInterviewDataExportView(Guid interviewId,
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
            return exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure, interview);
        }

        private string GetUserRole(UserDocument user)
        {
            if (user == null || !user.Roles.Any())
                return FileBasedDataExportRepositoryWriterMessages.UnknownRole;
            var firstRole = user.Roles.First();
            switch (firstRole)
            {
                case UserRoles.Operator:
                    return FileBasedDataExportRepositoryWriterMessages.Interviewer;
                case UserRoles.Supervisor:
                    return FileBasedDataExportRepositoryWriterMessages.Supervisor;
                case UserRoles.Headquarter:
                    return FileBasedDataExportRepositoryWriterMessages.Headquarter;
                case UserRoles.Administrator:
                    return FileBasedDataExportRepositoryWriterMessages.Administrator;
            }
            return FileBasedDataExportRepositoryWriterMessages.UnknownRole;
        }

        private string GetUserName(UserDocument responsible)
        {
            var userName = responsible != null
                ? responsible.UserName
                : FileBasedDataExportRepositoryWriterMessages.UnknownUser;
            return userName;
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
