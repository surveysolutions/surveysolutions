using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Resources;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class ReadSideToTabularFormatExportService : ITabularFormatExportService
    {
        private readonly string commentsFileName = "interview_comments";
        private readonly string interviewActionsFileName = "interview_actions";
        private readonly string[] actionFileColumns = new[] { "InterviewId", "Action", "Originator", "Role", "Date", "Time" };

        private readonly string parentId = "ParentId";
        private readonly string dataFileExtension = "tab";

        private readonly string separator;
        private readonly int returnRecordLimit;

        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly IJsonUtils jsonUtils;

        private readonly IQueryableReadSideRepositoryReader<InterviewExportedDataRecord> interviewExportedDataStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewActionsDataStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;

        public ReadSideToTabularFormatExportService(
            ITransactionManagerProvider transactionManagerProvider, 
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriterFactory csvWriterFactory, 
            IJsonUtils jsonUtils, 
            InterviewDataExportSettings interviewDataExportSettings,
            IQueryableReadSideRepositoryReader<InterviewExportedDataRecord> interviewExportedDataStorage, 
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewActionsDataStorage, 
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage, 
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriterFactory = csvWriterFactory;
            this.interviewExportedDataStorage = interviewExportedDataStorage;
            this.interviewActionsDataStorage = interviewActionsDataStorage;
            this.interviewCommentariesStorage = interviewCommentariesStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.jsonUtils = jsonUtils;

            this.separator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            this.returnRecordLimit = interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

        }

        public async Task ExportInterviewsInTabularFormatAsync(Guid questionnaireId, long questionnaireVersion,
            string basePath)
        {
            Expression<Func<InterviewCommentaries, bool>> whereClauseForComments =
                interviewComments =>
                    interviewComments.QuestionnaireId == questionnaireId.FormatGuid() &&
                    interviewComments.QuestionnaireVersion == questionnaireVersion;


            Expression<Func<InterviewStatuses, bool>> whereClauseForAction =
                interviewComments =>
                    interviewComments.QuestionnaireId == questionnaireId &&
                    interviewComments.QuestionnaireVersion == questionnaireVersion;


            Expression<Func<InterviewExportedDataRecord, bool>> whereClauseForInterviews =
                (i) => i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == questionnaireVersion;

            await
                ExportInterviewsInTabularFormatImplAsync(whereClauseForComments, whereClauseForAction,
                    whereClauseForInterviews, questionnaireId, questionnaireVersion, basePath);
        }

        public async Task ExportApprovedInterviewsInTabularFormatAsync(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            Expression<Func<InterviewCommentaries, bool>> whereClauseForComments =
               interviewCommentaries =>
                   interviewCommentaries.QuestionnaireId == questionnaireId.FormatGuid() &&
                   interviewCommentaries.QuestionnaireVersion == questionnaireVersion &&
                   interviewCommentaries.IsApprovedByHQ;


            Expression<Func<InterviewStatuses, bool>> whereClauseForAction =
                interviewWithStatusHistory =>
                    interviewWithStatusHistory.QuestionnaireId == questionnaireId &&
                    interviewWithStatusHistory.QuestionnaireVersion == questionnaireVersion &&
                    interviewWithStatusHistory.InterviewCommentedStatuses.Select(s => s.Status)
                        .Any(s => s == InterviewExportedAction.ApprovedByHeadquarter);


            Expression<Func<InterviewExportedDataRecord, bool>> whereClauseForInterviews =
                (interviewExportedDataRecord) => 
                    interviewExportedDataRecord.QuestionnaireId == questionnaireId && 
                    interviewExportedDataRecord.QuestionnaireVersion == questionnaireVersion &&
                    interviewExportedDataRecord.LastAction == InterviewExportedAction.ApprovedByHeadquarter;

            await
                ExportInterviewsInTabularFormatImplAsync(whereClauseForComments, whereClauseForAction,
                    whereClauseForInterviews, questionnaireId, questionnaireVersion, basePath);
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            QuestionnaireExportStructure questionnaireExportStructure =
              this.transactionManagerProvider.GetTransactionManager()
                  .ExecuteInQueryTransaction(
                      () =>
                          questionnaireExportStructureStorage.AsVersioned()
                              .Get(questionnaireId.FormatGuid(), questionnaireVersion));

            if (questionnaireExportStructure == null)
                return;

            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
        }

        public string[] GetTabularDataFilesFromFolder(string basePath)
        {
            var filesInDirectory = fileSystemAccessor.GetFilesInDirectory(basePath).Where(fileName => fileName.EndsWith("." + dataFileExtension)).ToArray();
            return filesInDirectory;
        }

        private async Task ExportInterviewsInTabularFormatImplAsync(
            Expression<Func<InterviewCommentaries, bool>> whereClauseForComments,
            Expression<Func<InterviewStatuses, bool>> whereClauseForAction,
            Expression<Func<InterviewExportedDataRecord, bool>> whereClauseForInterviews,
            Guid questionnaireId,
            long questionnaireVersion,
            string basePath)
        {
            QuestionnaireExportStructure questionnaireExportStructure =
                this.transactionManagerProvider.GetTransactionManager()
                    .ExecuteInQueryTransaction(
                        () =>
                            questionnaireExportStructureStorage.AsVersioned()
                                .Get(questionnaireId.FormatGuid(), questionnaireVersion));

            if (questionnaireExportStructure == null)
                return;

            await Task.WhenAll(
                this.ExportCommentsInTabularFormatAsync(questionnaireExportStructure, whereClauseForComments, basePath),
                this.ExportActionsInTabularFormatAsync(whereClauseForAction, basePath),
                this.ExportInterviewsInTabularFormatAsync(questionnaireExportStructure, whereClauseForInterviews,
                    basePath));
        }

        private async Task ExportCommentsInTabularFormatAsync(
            QuestionnaireExportStructure questionnaireExportStructure,
            Expression<Func<InterviewCommentaries, bool>> whereClauseForComments, 
            string basePath)
        {
            string commentsFilePath =
                fileSystemAccessor.CombinePath(basePath, this.CreateFormatDataFileName(commentsFileName));

            int maxRosterDepthInQuestionnaire =
                questionnaireExportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);

            bool hasAtLeastOneRoster =
                questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);

            var commentsHeader = new List<string>(){"Order", "Originator", "Role", "Date", "Time", "Variable"};

            if (hasAtLeastOneRoster)
                commentsHeader.Add("Roster");

            commentsHeader.Add("InterviewId");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                commentsHeader.Add(string.Format("Id{0}", i));
            }
            commentsHeader.Add("Comment");

            WriteData(commentsFilePath, new[] { commentsHeader.ToArray() });

            foreach (var queryCommentsChunk in this.GetTasksForQueryCommentsByChunks(whereClauseForComments, maxRosterDepthInQuestionnaire, hasAtLeastOneRoster))
            {
                var commentRecords = await queryCommentsChunk;

                WriteData(commentsFilePath, commentRecords);
            }
        }

        private async Task ExportActionsInTabularFormatAsync(Expression<Func<InterviewStatuses, bool>> whereClauseForAction, string basePath)
        {
            var actionFilePath =
             fileSystemAccessor.CombinePath(basePath, this.CreateFormatDataFileName(interviewActionsFileName));

            WriteData(actionFilePath, new[] {actionFileColumns});

            foreach (var queryActionsChunk in this.GetTasksForQueryActionsByChunks(whereClauseForAction))
            {
                var actionsRecords = await queryActionsChunk;

                WriteData(actionFilePath, actionsRecords);
            }
        }

        private async Task ExportInterviewsInTabularFormatAsync(
            QuestionnaireExportStructure questionnaireExportStructure, Expression<Func<InterviewExportedDataRecord, bool>> whereClauseForInterviews, string basePath)
        {
            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);

            foreach (var queryInterviewsChunk in this.GetTasksForQueryInterviewsByChunks(whereClauseForInterviews))
            {
                var actionsRecords = await queryInterviewsChunk;

                foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
                {
                    var dataByTheLevelFilePath =
                        this.fileSystemAccessor.CombinePath(basePath, this.CreateFormatDataFileName(level.LevelName));

                    if (actionsRecords.ContainsKey(level.LevelName))
                        WriteData(dataByTheLevelFilePath, actionsRecords[level.LevelName]);
                }
            }
        }

        private void CreateDataSchemaForInterviewsInTabular(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                var dataByTheLevelFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, this.CreateFormatDataFileName(level.LevelName));

                var interviewLevelHeader = new List<string>();

                interviewLevelHeader.Add(level.LevelIdColumnName);

                if (level.IsTextListScope)
                {
                    foreach (var name in level.ReferencedNames)
                    {
                        interviewLevelHeader.Add(name);
                    }
                }

                foreach (ExportedHeaderItem question in level.HeaderItems.Values)
                {
                    foreach (var columnName in question.ColumnNames)
                    {
                        interviewLevelHeader.Add(columnName);
                    }
                }

                for (int i = 0; i < level.LevelScopeVector.Length; i++)
                {
                    interviewLevelHeader.Add(string.Format("{0}{1}", parentId, i + 1));
                }

                WriteData(dataByTheLevelFilePath, new[] { interviewLevelHeader.ToArray() });
            }
        }

        private IEnumerable<Task<Dictionary<string, List<string[]>>>> GetTasksForQueryInterviewsByChunks(Expression<Func<InterviewExportedDataRecord, bool>> whereClauseForInterviews)
        {
            return this.GetTasksForQueryByChunks(
                (skip) =>
                    this.QueryInterviewsChunkFromReadSide(whereClauseForInterviews, skip),
                () => interviewExportedDataStorage.Query(
                    _ => _.Where(whereClauseForInterviews).Count()));
        }

        private IEnumerable<Task<string[][]>> GetTasksForQueryCommentsByChunks(Expression<Func<InterviewCommentaries, bool>> whereClauseForComments,
            int maxRosterDepthInQuestionnaire, bool hasAtLeastOneRoster)
        {
            return this.GetTasksForQueryByChunks(
                (skip) =>
                    this.QueryCommentsChunkFromReadSide(whereClauseForComments, skip, maxRosterDepthInQuestionnaire, hasAtLeastOneRoster),
                () => interviewCommentariesStorage.Query(
                    _ => _.Where(whereClauseForComments).SelectMany(x => x.Commentaries).Count()));
        }

        private IEnumerable<Task<string[][]>> GetTasksForQueryActionsByChunks(Expression<Func<InterviewStatuses, bool>> whereClauseForAction)
        {
            return this.GetTasksForQueryByChunks(
                (skip) => this.QueryActionsChunkFromReadSide(whereClauseForAction, skip),
                () => interviewActionsDataStorage.Query(
                    _ => _.Where(whereClauseForAction).SelectMany(x => x.InterviewCommentedStatuses).Count()));
        }

        private Dictionary<string, List<string[]>> QueryInterviewsChunkFromReadSide(
            Expression<Func<InterviewExportedDataRecord, bool>> whereClauseForInterviews, int skip)
        {
            List<InterviewExportedDataRecord> interviewDatas = interviewExportedDataStorage.Query(
                _ =>
                    _.Where(whereClauseForInterviews).OrderBy(i=>i.InterviewId).Skip(skip).Take(returnRecordLimit)
                        .ToList());

            var result = new Dictionary<string, List<string[]>>();

            foreach (var interviewExportedDataRecord in interviewDatas)
            {
                var data = jsonUtils.Deserialize<Dictionary<string, string[]>>(interviewExportedDataRecord.Data);
                foreach (var levelName in data.Keys)
                {
                    foreach (var dataByLevel in data[levelName])
                    {
                        if (!result.ContainsKey(levelName))
                            result.Add(levelName, new List<string[]>());
                        result[levelName].Add(dataByLevel.Split(ExportFileSettings.SeparatorOfExportedDataFile));
                    }
                }
            }
            return result;
        }

        private string[][] QueryCommentsChunkFromReadSide(Expression<Func<InterviewCommentaries, bool>> queryComments, int skip, int maxRosterDepthInQuestionnaire, bool hasAtLeastOneRoster)
        {
            var comments = interviewCommentariesStorage.Query(
                            _ =>
                                _.Where(queryComments)
                                    .SelectMany(
                                        interviewComments => interviewComments.Commentaries,
                                        (interview, comment) => new {interview.InterviewId, Comments = comment})
                                    .Select(
                                        i =>
                                            new
                                            {
                                                i.InterviewId,
                                                i.Comments.CommentSequence,
                                                i.Comments.OriginatorName,
                                                i.Comments.OriginatorRole,
                                                i.Comments.Timestamp,
                                                i.Comments.Variable,
                                                i.Comments.Roster,
                                                i.Comments.RosterVector,
                                                i.Comments.Comment
                                            }).OrderBy(i=>i.Timestamp).Skip(skip).Take(returnRecordLimit).ToList());

            var result = new List<string[]>();

            foreach (var interview in comments)
            {
                var resultRow = new List<string>();
                resultRow.Add(interview.CommentSequence.ToString());
                resultRow.Add(interview.OriginatorName);
                resultRow.Add(GetUserRole(interview.OriginatorRole));
                resultRow.Add(interview.Timestamp.ToString("d", CultureInfo.InvariantCulture));
                resultRow.Add(interview.Timestamp.ToString("T", CultureInfo.InvariantCulture));

                resultRow.Add(interview.Variable);

                if (hasAtLeastOneRoster)
                    resultRow.Add(interview.Roster);

                resultRow.Add(interview.InterviewId);

                for (int i = 0; i < maxRosterDepthInQuestionnaire; i++)
                {
                    resultRow.Add(interview.RosterVector.Length > i ? interview.RosterVector[i].ToString() : "");
                }

                resultRow.Add(interview.Comment);

                result.Add(resultRow.ToArray());
            }
            return result.ToArray();
        }

        private string[][] QueryActionsChunkFromReadSide(Expression<Func<InterviewStatuses, bool>> queryActions, int skip)
        {
            var interviews =
              interviewActionsDataStorage.Query(
                  _ =>
                      _.Where(queryActions)
                          .SelectMany(
                              interviewWithStatusHistory => interviewWithStatusHistory.InterviewCommentedStatuses,
                              (interview, status) => new { interview.InterviewId, StatusHistory = status })
                          .Select(
                              i =>
                                  new
                                  {
                                      i.InterviewId,
                                      i.StatusHistory.Status,
                                      i.StatusHistory.StatusChangeOriginatorName,
                                      i.StatusHistory.StatusChangeOriginatorRole,
                                      i.StatusHistory.Timestamp

                                  }).OrderBy(i => i.Timestamp).Skip(skip).Take(returnRecordLimit).ToList());

            var result = new List<string[]>();

            foreach (var interview in interviews)
            {
                var resultRow = new List<string>();
                resultRow.Add(interview.InterviewId);
                resultRow.Add(interview.Status.ToString());
                resultRow.Add(interview.StatusChangeOriginatorName);
                resultRow.Add(GetUserRole(interview.StatusChangeOriginatorRole));
                resultRow.Add(interview.Timestamp.ToString("d", CultureInfo.InvariantCulture));
                resultRow.Add(interview.Timestamp.ToString("T", CultureInfo.InvariantCulture));
                result.Add(resultRow.ToArray());
            }
            return result.ToArray();
        }

        private IEnumerable<Task<T>> GetTasksForQueryByChunks<T>(Func<int, T> dataQuery, Func<int> countQuery)
        {
            var countOfAllRecords =
                this.transactionManagerProvider.GetTransactionManager()
                    .ExecuteInQueryTransaction(countQuery);

            int skip = 0;

            while (skip < countOfAllRecords)
            {
                var skipAtCurrentIteration = skip;

                yield return Task.Run(() => this.transactionManagerProvider.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => dataQuery(skipAtCurrentIteration)));

                skip = skip + returnRecordLimit;
            }
        }

        private string CreateFormatDataFileName(string fileName)
        {
            return String.Format("{0}.{1}", fileName, dataFileExtension);
        }

        private string GetUserRole(UserRoles userRole)
        {
            switch (userRole)
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

        private void WriteData(string filePath, IEnumerable<string[]> records)
        {
            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(filePath, true))
            using (var tabWriter = this.csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
            {
                foreach (var dataRow in records)
                {
                    foreach (var cell in dataRow)
                    {
                        tabWriter.WriteField(cell);
                    }

                    tabWriter.NextRecord();
                }
            }
        }
    }
}