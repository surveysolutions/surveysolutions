using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Ionic.Zlib;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class FileBasedDataExportRepositoryWriter : IDataExportRepositoryWriter
    {
        private readonly IDataExportWriter dataExportWriter;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IFilebaseExportDataAccessor filebaseExportDataAccessor;

        private readonly ILogger logger;

        private readonly IExportViewFactory exportViewFactory;

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPlainInterviewFileStorage plainFileRepository;

        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter;
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        private const string UnknownUserRole = "<UNKNOWN ROLE>";
        private bool isCacheEnabled = false;
        private readonly Dictionary<string, QuestionnaireExportEntity> cache = new Dictionary<string, QuestionnaireExportEntity>();

        public FileBasedDataExportRepositoryWriter(
            IReadSideRepositoryCleanerRegistry cleanerRegistry,
            IDataExportWriter dataExportWriter,
            IEnvironmentContentService environmentContentService,
            IFileSystemAccessor fileSystemAccessor, ILogger logger, IPlainInterviewFileStorage plainFileRepository,
            IReadSideRepositoryWriterRegistry writerRegistry, IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter,
            IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            IReadSideRepositoryWriter<UserDocument> users, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter,
            IExportViewFactory exportViewFactory, IFilebaseExportDataAccessor filebaseExportDataAccessor)
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
            this.filebaseExportDataAccessor = filebaseExportDataAccessor;

            cleanerRegistry.Register(this);
            writerRegistry.Register(this);
        }

        public void Clear()
        {
            filebaseExportDataAccessor.CleanExportDataFolder();
            filebaseExportDataAccessor.CleanExportFileFolder();
        }

        public void CreateExportStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure)
        {
            this.CreateExportedDataStructure(questionnaireExportStructure);
            this.CreateExportedFileStructure(questionnaireExportStructure);
        }

        public void AddExportedDataByInterview(Guid interviewId)
        {
            if (isCacheEnabled)
            {
                this.GetOrCreateQuestionnaireExportEntityByInterviewId(interviewId).InterviewIds.Add(interviewId);
                return;
            }

            var interviewDataExportView = this.CreateInterviewDataExportView(interviewId);

            if (interviewDataExportView==null)
                return;

            this.AddExportedDataByInterviewImpl(interviewDataExportView);
        }

        public void AddInterviewAction(InterviewExportedAction action, Guid interviewId, Guid userId, DateTime timestamp)
        {
            if (isCacheEnabled)
            {
                this.GetOrCreateQuestionnaireExportEntityByInterviewId(interviewId).Actions.Add(new ActionCacheEntity(interviewId, action, userId, timestamp));
                return;
            }

            var interviewSummary = interviewSummaryWriter.GetById(interviewId);

            if (interviewSummary == null)
                return;

            this.AddInterviewActionImpl(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion,
                 CreateInterviewAction(action, interviewId, userId, timestamp));
        }

        public void DeleteInterview(Guid interviewId)
        {
            if (isCacheEnabled)
            {
                this.GetOrCreateQuestionnaireExportEntityByInterviewId(interviewId).InterviewIds.Remove(interviewId);
                return;
            }

            var interviewSummary = interviewSummaryWriter.GetById(interviewId);
            if (interviewSummary == null)
                return;

            this.DeleteInterviewImpl(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion, interviewId);
        }

        public void DeleteExportedDataForQuestionnaireVersion(Guid questionnaireId, long questionnaireVersion)
        {
            var dataFolderForTemplatePath = this.filebaseExportDataAccessor.GetFolderPathOfDataByQuestionnaireOrThrow(questionnaireId, questionnaireVersion);

            if (isCacheEnabled)
            {
                cache.Remove(dataFolderForTemplatePath);
            }

            if (this.fileSystemAccessor.IsDirectoryExists(dataFolderForTemplatePath))
            {
                this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);
            }

            var filesFolderForTemplatePath = this.filebaseExportDataAccessor.GetFolderPathOfFilesByQuestionnaireOrThrow(questionnaireId, questionnaireVersion);

            if (this.fileSystemAccessor.IsDirectoryExists(filesFolderForTemplatePath))
            {
                this.fileSystemAccessor.DeleteDirectory(filesFolderForTemplatePath);
            }
        }

        public void EnableCache()
        {
            this.isCacheEnabled = true;
        }

        public void DisableCache()
        {
            var cachedEntities = this.cache.Keys.ToArray();
            foreach (var cachedEntity in cachedEntities)
            {
                var entity = cache[cachedEntity];
                var exportStructure = questionnaireExportStructureWriter.GetById(entity.QuestionnaireId, entity.QuestionnaireVersion);
                if (exportStructure != null)
                {
                    this.dataExportWriter.BatchInsert(cachedEntity,
                        cache[cachedEntity].InterviewIds.Select(i => CreateInterviewDataExportView(i, exportStructure)).Where(i => i != null),
                        entity.Actions.Select(a => this.CreateInterviewAction(a.Action, a.InterviewId, a.UserId, a.Timestamp)).Where(a => a != null));
                }
                this.cache.Remove(cachedEntity);
            }
            this.isCacheEnabled = false;
        }

        public string GetReadableStatus()
        {
            int cachedEntities = this.cache.Count;

            return string.Format("cache {0,8};    cached: {1,3};    not stored: {2,3}",
                this.isCacheEnabled ? "enabled" : "disabled",
                cachedEntities,
                cachedEntities);
        }

        public Type ViewType { get { return typeof(InterviewDataExportView); } }

        private void CreateExportedDataStructure(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var dataFolderForTemplatePath = this.filebaseExportDataAccessor.CreateExportDataFolder(questionnaireExportStructure.QuestionnaireId,
                    questionnaireExportStructure.Version);

            this.dataExportWriter.CreateStructure(questionnaireExportStructure, dataFolderForTemplatePath);

            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                this.environmentContentService.CreateContentOfAdditionalFile(headerStructureForLevel, ExportFileSettings.GetContentFileName(headerStructureForLevel.LevelName), dataFolderForTemplatePath);
            }
        }

        private void CreateExportedFileStructure(QuestionnaireExportStructure questionnaireExportStructure)
        {
            this.filebaseExportDataAccessor.CreateExportFileFolder(questionnaireExportStructure.QuestionnaireId,
                questionnaireExportStructure.Version);
        }

        private void DeleteInterviewImpl(Guid questionnaireId, long questionnaireVersion, Guid interviewId)
        {
            var dataFolderForTemplatePath = this.filebaseExportDataAccessor.GetFolderPathOfDataByQuestionnaireOrThrow(questionnaireId,
                questionnaireVersion);

            this.dataExportWriter.DeleteInterviewRecords(dataFolderForTemplatePath, interviewId);

            var filesFolderForInterview = this.filebaseExportDataAccessor.GetFolderPathOfFilesByQuestionnaireForInterview(questionnaireId,
                questionnaireVersion, interviewId);

            if (fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                fileSystemAccessor.DeleteDirectory(filesFolderForInterview);
        }

        private void AddExportedDataByInterviewImpl(InterviewDataExportView interviewDataExportView)
        {
            var dataFolderForTemplatePath = this.filebaseExportDataAccessor.GetFolderPathOfDataByQuestionnaireOrThrow(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);

            this.dataExportWriter.AddOrUpdateInterviewRecords(interviewDataExportView, dataFolderForTemplatePath);

            var filesFolderForInterview = this.filebaseExportDataAccessor.GetFolderPathOfFilesByQuestionnaireForInterview(interviewDataExportView.TemplateId,
                interviewDataExportView.TemplateVersion, interviewDataExportView.InterviewId);

            if (fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                fileSystemAccessor.DeleteDirectory(filesFolderForInterview);

            fileSystemAccessor.CreateDirectory(filesFolderForInterview);

            var questionsWithAnswersOnMultimediaQuestions = this.GetAllMultimediaQuestionFileNames(interviewDataExportView);

            foreach (var questionWithAnswersOnMultimediaQuestions in questionsWithAnswersOnMultimediaQuestions)
            {
                var fileContent = plainFileRepository.GetInterviewBinaryData(interviewDataExportView.InterviewId,
                    questionWithAnswersOnMultimediaQuestions);

                if (fileContent == null || fileContent.Length == 0)
                {
                    logger.Error(string.Format("file content is missing for file name {0} interview {1}", questionWithAnswersOnMultimediaQuestions, interviewDataExportView.InterviewId));
                    continue;
                }

                this.fileSystemAccessor.WriteAllBytes(this.fileSystemAccessor.CombinePath(filesFolderForInterview, questionWithAnswersOnMultimediaQuestions), fileContent);
            }
        }

        private void AddInterviewActionImpl(Guid questionnaireId, long questionnaireVersion, InterviewActionExportView action)
        {
            var dataFolderForTemplatePath = this.filebaseExportDataAccessor.GetFolderPathOfDataByQuestionnaireOrThrow(questionnaireId, questionnaireVersion);

            this.dataExportWriter.AddActionRecord(action, dataFolderForTemplatePath);
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

        private QuestionnaireExportEntity GetOrCreateQuestionnaireExportEntityByInterviewId(Guid interviewId)
        {
            var interviewSummary = interviewSummaryWriter.GetById(interviewId);
            if (interviewSummary == null)
                throw new InterviewDataExportException(
                  string.Format("data files are absent for interview with id '{0}'",
                      interviewId));

            var dataFolderForTemplatePath = this.filebaseExportDataAccessor.GetFolderPathOfDataByQuestionnaireOrThrow(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            if (!cache.ContainsKey(dataFolderForTemplatePath))
                cache.Add(dataFolderForTemplatePath, new QuestionnaireExportEntity(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion));
            return cache[dataFolderForTemplatePath];
        }

        private InterviewActionExportView CreateInterviewAction(InterviewExportedAction action, Guid interviewId, Guid userId, DateTime timestamp)
        {
            UserDocument responsible = this.users.GetById(userId);

            if (responsible == null)
                return null;

            var userName = this.GetUserName(responsible);

            return
                new InterviewActionExportView(interviewId.FormatGuid(), action, userName, timestamp, this.GetUserRole(responsible));
        }

        private InterviewDataExportView CreateInterviewDataExportView(Guid interviewId, QuestionnaireExportStructure questionnaireExportStructure=null)
        {
            var interview = interviewDataWriter.GetById(interviewId);
            if(interview==null || interview.Document==null || interview.Document.IsDeleted)
                return null;

            if (questionnaireExportStructure == null)
            {
                questionnaireExportStructure = questionnaireExportStructureWriter.GetById(interview.Document.QuestionnaireId, interview.Document.QuestionnaireVersion);
                if (questionnaireExportStructure == null)
                    return null;
            }
            return exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure, interview.Document);
        }

        private string GetUserRole(UserDocument user)
        {
            if (user == null || !user.Roles.Any())
                return UnknownUserRole;
            var firstRole = user.Roles.First();
            switch (firstRole)
            {
                case UserRoles.Operator: return "Interviewer";
                case UserRoles.Supervisor: return "Supervisor";
                case UserRoles.Headquarter: return "Headquarter";
            }
            return UnknownUserRole;
        }

        private string GetUserName(UserDocument responsible)
        {
            var userName = responsible != null ? responsible.UserName : "<UNKNOWN USER>";
            return userName;
        }

        #region cache interral classes

        private class ActionCacheEntity
        {
            public ActionCacheEntity(Guid interviewId, InterviewExportedAction action, Guid userId, DateTime timestamp)
            {
                this.InterviewId = interviewId;
                this.Action = action;
                this.UserId = userId;
                this.Timestamp = timestamp;
            }

            public Guid InterviewId { get; private set; }
            public InterviewExportedAction Action { get; private set; }
            public Guid UserId { get; private set; }
            public DateTime Timestamp { get; private set; }
        }

        private class QuestionnaireExportEntity
        {
            public QuestionnaireExportEntity(Guid questionnaireId, long questionnaireVersion)
            {
                this.QuestionnaireId = questionnaireId;
                this.QuestionnaireVersion = questionnaireVersion;
                this.InterviewIds = new HashSet<Guid>();
                this.Actions = new List<ActionCacheEntity>();
            }

            public Guid QuestionnaireId { get; private set; }
            public long QuestionnaireVersion { get; private set; }
            public HashSet<Guid> InterviewIds { get; private set; }
            public List<ActionCacheEntity> Actions { get; private set; }
        }

        #endregion

    }
    
}
