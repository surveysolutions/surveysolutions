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
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        private readonly FileBasedDataExportRepositorySettings settings;
        private bool isCacheEnabled = false;
        private readonly Dictionary<string, QuestionnaireExportEntity> cache = new Dictionary<string, QuestionnaireExportEntity>();

        public FileBasedDataExportRepositoryWriter(
            IDataExportWriter dataExportWriter,
            IEnvironmentContentService environmentContentService,
            IFileSystemAccessor fileSystemAccessor, ILogger logger, IPlainInterviewFileStorage plainFileRepository, IReadSideKeyValueStorage<InterviewData> interviewDataWriter,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            IReadSideRepositoryWriter<UserDocument> users, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter,
            IExportViewFactory exportViewFactory, IFilebasedExportedDataAccessor filebasedExportedDataAccessor, FileBasedDataExportRepositorySettings settings)
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
            this.settings = settings;
        }

        public void Clear()
        {
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
            if (isCacheEnabled)
            {
                var cacheByInterview = this.GetOrCreateQuestionnaireExportEntityByInterviewId(interviewId);
                cacheByInterview.InterviewIds.Add(interviewId);
                cacheByInterview.InterviewForDeleteIds.Remove(interviewId);

                ReduceCacheIfNeeded(cacheByInterview.PathToDataBase);
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

            if (interviewSummary == null || interviewSummary.IsDeleted)
                return;

            this.AddInterviewActionImpl(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion,
                 CreateInterviewAction(action, interviewId, userId, timestamp));
        }

        public void DeleteInterview(Guid interviewId)
        {
            if (isCacheEnabled)
            {
                QuestionnaireExportEntity cacheByInterview = this.GetOrCreateQuestionnaireExportEntityByInterviewId(interviewId);
                cacheByInterview.InterviewIds.Remove(interviewId);
                cacheByInterview.InterviewForDeleteIds.Add(interviewId);
                return;
            }

            var interviewSummary = interviewSummaryWriter.GetById(interviewId);
            if (interviewSummary == null)
                return;

            this.DeleteInterviewImpl(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion, interviewId);
        }

        public void DeleteExportedDataForQuestionnaireVersion(Guid questionnaireId, long questionnaireVersion)
        {
            var dataFolderForTemplatePath = this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId,
                questionnaireVersion);

            if (isCacheEnabled)
            {
                cache.Remove(dataFolderForTemplatePath);
            }

            this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);

            var filesFolderForTemplatePath = this.filebasedExportedDataAccessor.GetFolderPathOfFilesByQuestionnaire(questionnaireId,
                questionnaireVersion);

            this.fileSystemAccessor.DeleteDirectory(filesFolderForTemplatePath);
        }

        public void EnableCache()
        {
            this.isCacheEnabled = true;
        }

        public bool IsCacheEnabled { get { return this.isCacheEnabled; } }

        public void DisableCache()
        {
            var cachedEntities = this.cache.Keys.ToArray();
            foreach (var cachedEntity in cachedEntities)
            {
                if(cache.ContainsKey(cachedEntity))
                    ReduceCache(cache[cachedEntity]);
            }
            this.isCacheEnabled = false;
        }

        public string GetReadableStatus()
        {
            return string.Format(
                "File Export ._. | cache {1}{0}updates db: {2}{0}insert interviews: {3}, delete interviews: {4}{0}insert actions: {5}",
                Environment.NewLine,
                this.isCacheEnabled ? "enabled" : "disabled",
                cache.Count,
                cache.Values.Sum(c=>c.InterviewIds.Count),
                cache.Values.Sum(c => c.InterviewForDeleteIds.Count),
                cache.Values.Sum(c => c.Actions.Count));
        }

        public Type ViewType { get { return dataExportWriter.GetType(); } }

        private void ReduceCacheIfNeeded(string entityDbPath)
        {
            var entity = cache[entityDbPath];
            if (this.IsCacheLimitReached(entity))
            {
                this.ReduceCache(entity);
            }
        }

        private bool IsCacheLimitReached(QuestionnaireExportEntity entity)
        {
            return entity.InterviewIds.Count >= this.settings.MaxCountOfCachedEntities;
        }

        private void ReduceCache(QuestionnaireExportEntity entity)
        {
            var exportStructure = questionnaireExportStructureWriter.AsVersioned().Get(entity.QuestionnaireId.FormatGuid(), entity.QuestionnaireVersion);
            if (exportStructure == null)
            {
                return;
            }

            try
            {
                this.dataExportWriter.BatchInsert(entity.PathToDataBase,
                    entity.InterviewIds.Select(i => CreateInterviewDataExportView(i, exportStructure))
                        .Where(i => i != null),
                    entity.Actions.Select(a => this.CreateInterviewAction(a.Action, a.InterviewId, a.UserId, a.Timestamp))
                        .Where(a => a != null), entity.InterviewForDeleteIds);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }

            this.cache.Remove(entity.PathToDataBase);
        }

        private void CreateExportedDataStructure(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var dataFolderForTemplatePath = this.filebasedExportedDataAccessor.CreateExportDataFolder(questionnaireExportStructure.QuestionnaireId,
                    questionnaireExportStructure.Version);

            this.dataExportWriter.CreateStructure(questionnaireExportStructure, dataFolderForTemplatePath);

            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                this.environmentContentService.CreateContentOfAdditionalFile(headerStructureForLevel, ExportFileSettings.GetContentFileName(headerStructureForLevel.LevelName), dataFolderForTemplatePath);
            }
        }

        private void CreateExportedFileStructure(QuestionnaireExportStructure questionnaireExportStructure)
        {
            this.filebasedExportedDataAccessor.CreateExportFileFolder(questionnaireExportStructure.QuestionnaireId,
                questionnaireExportStructure.Version);
        }

        private void DeleteInterviewImpl(Guid questionnaireId, long questionnaireVersion, Guid interviewId)
        {
            var dataFolderForTemplatePath = this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId,
                questionnaireVersion);

            this.dataExportWriter.DeleteInterviewRecords(dataFolderForTemplatePath, interviewId);

            var filesFolderForInterview = this.filebasedExportedDataAccessor.GetFolderPathOfFilesByQuestionnaireForInterview(questionnaireId,
                questionnaireVersion, interviewId);

            if (fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                fileSystemAccessor.DeleteDirectory(filesFolderForInterview);
        }

        private void AddExportedDataByInterviewImpl(InterviewDataExportView interviewDataExportView)
        {
            var dataFolderForTemplatePath = this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);

            this.dataExportWriter.AddOrUpdateInterviewRecords(interviewDataExportView, dataFolderForTemplatePath);

            var filesFolderForInterview = this.filebasedExportedDataAccessor.GetFolderPathOfFilesByQuestionnaireForInterview(interviewDataExportView.TemplateId,
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
                    logger.Error(string.Format(FileBasedDataExportRepositoryWriterMessages.FileContentIsMissingForFileNameAndInterviewFormat, questionWithAnswersOnMultimediaQuestions, interviewDataExportView.InterviewId));
                    continue;
                }

                this.fileSystemAccessor.WriteAllBytes(this.fileSystemAccessor.CombinePath(filesFolderForInterview, questionWithAnswersOnMultimediaQuestions), fileContent);
            }
        }

        private void AddInterviewActionImpl(Guid questionnaireId, long questionnaireVersion, InterviewActionExportView action)
        {
            var dataFolderForTemplatePath = this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId, questionnaireVersion);

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
                    string.Format(FileBasedDataExportRepositoryWriterMessages.InterviewWithIdIsAbsentFormat,
                        interviewId));

            var dataFolderForTemplatePath = this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);

            if (!cache.ContainsKey(dataFolderForTemplatePath))
            {
                cache.Add(dataFolderForTemplatePath, 
                    new QuestionnaireExportEntity(dataFolderForTemplatePath, interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion));
            }

            return cache[dataFolderForTemplatePath];
        }

        private InterviewActionExportView CreateInterviewAction(InterviewExportedAction action, Guid interviewId, Guid userId, DateTime timestamp)
        {
            UserDocument responsible = this.users.GetById(userId);

            var userName = this.GetUserName(responsible);

            return
                new InterviewActionExportView(interviewId.FormatGuid(), action, userName, timestamp, this.GetUserRole(responsible));
        }

        private InterviewDataExportView CreateInterviewDataExportView(Guid interviewId, QuestionnaireExportStructure questionnaireExportStructure=null)
        {
            var interview = interviewDataWriter.GetById(interviewId);
            if(interview==null  || interview.IsDeleted)
                return null;

            if (questionnaireExportStructure == null)
            {
                questionnaireExportStructure = questionnaireExportStructureWriter.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(), interview.QuestionnaireVersion);
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
                case UserRoles.Operator: return  FileBasedDataExportRepositoryWriterMessages.Interviewer;
                case UserRoles.Supervisor: return  FileBasedDataExportRepositoryWriterMessages.Supervisor;
                case UserRoles.Headquarter: return  FileBasedDataExportRepositoryWriterMessages.Headquarter;
            }
            return FileBasedDataExportRepositoryWriterMessages.UnknownRole;
        }

        private string GetUserName(UserDocument responsible)
        {
            var userName = responsible != null ? responsible.UserName : FileBasedDataExportRepositoryWriterMessages.UnknownUser;
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
            public QuestionnaireExportEntity(string pathToDataBase, Guid questionnaireId, long questionnaireVersion)
            {
                this.PathToDataBase = pathToDataBase;
                this.QuestionnaireId = questionnaireId;
                this.QuestionnaireVersion = questionnaireVersion;
                this.InterviewIds = new HashSet<Guid>();
                this.InterviewForDeleteIds = new HashSet<Guid>();
                this.Actions = new List<ActionCacheEntity>();
            }

            public Guid QuestionnaireId { get; private set; }
            public long QuestionnaireVersion { get; private set; }
            public string PathToDataBase { get; private set; }
            public HashSet<Guid> InterviewIds { get; private set; }
            public HashSet<Guid> InterviewForDeleteIds { get; private set; }
            public List<ActionCacheEntity> Actions { get; private set; }
        }

        #endregion

    }
    
}
