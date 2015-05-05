using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlToTabDataExportService : IDataExportService
    {
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly string separator;
        private readonly Func<string, string> createDataFileName;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISqlDataAccessor sqlDataAccessor;
        private readonly string parentId = "ParentId";
        private readonly IQueryableReadSideRepositoryReader<InterviewExportedDataRecord> interviewExportedDataStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewHistory> interviewActionsDataStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IJsonUtils jsonUtils;

        public SqlToTabDataExportService(IFileSystemAccessor fileSystemAccessor,
            ICsvWriterFactory csvWriterFactory, ISqlDataAccessor sqlDataAccessor,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            IQueryableReadSideRepositoryReader<InterviewExportedDataRecord> interviewExportedDataStorage,
            IQueryableReadSideRepositoryReader<InterviewHistory> interviewActionsDataStorage, 
            IJsonUtils jsonUtils, 
            ITransactionManagerProvider transactionManager)
        {
            this.csvWriterFactory = csvWriterFactory;
            this.sqlDataAccessor = sqlDataAccessor;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.interviewExportedDataStorage = interviewExportedDataStorage;
            this.interviewActionsDataStorage = interviewActionsDataStorage;
            this.jsonUtils = jsonUtils;
            this.transactionManager = transactionManager;
            this.createDataFileName = ExportFileSettings.GetContentFileName;
            this.separator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string targetFolder)
        {
            var structure = questionnaireExportStructureWriter.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);

            if (structure == null)
                return;

            foreach (var levels in structure.HeaderToLevelMap.Values)
            {
                var dataFilePath =
                    fileSystemAccessor.CombinePath(targetFolder, createDataFileName(levels.LevelName));

                using (var fileStream = fileSystemAccessor.OpenOrCreateFile(dataFilePath, true))
                using (var tabWriter = csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
                {
                    CreateHeaderForDataFile(tabWriter, levels);
                }
            }
        }

        public string[] GetDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            var allDataFolderPath = sqlDataAccessor.GetAllDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(allDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(allDataFolderPath);

            fileSystemAccessor.CreateDirectory(allDataFolderPath);

            return this.ExportToTabFile(questionnaireId, questionnaireVersion, allDataFolderPath);
        }

        public string[] GetDataFilesForQuestionnaireByInterviewsInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            var approvedDataFolderPath = sqlDataAccessor.GetApprovedDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(approvedDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(approvedDataFolderPath);

            fileSystemAccessor.CreateDirectory(approvedDataFolderPath);

            return this.ExportToTabFile(questionnaireId, questionnaireVersion,approvedDataFolderPath,
                InterviewExportedAction.ApproveByHeadquarter);
        }

        private void CreateHeaderForActionFile(ICsvWriterService fileWriter)
        {
            foreach (var actionFileColumn in sqlDataAccessor.ActionFileColumns)
            {
                fileWriter.WriteField(actionFileColumn);
            }
            fileWriter.NextRecord();
        }

        private void CreateHeaderForDataFile(ICsvWriterService fileWriter, HeaderStructureForLevel headerStructureForLevel)
        {
            fileWriter.WriteField(headerStructureForLevel.LevelIdColumnName);

            if (headerStructureForLevel.IsTextListScope)
            {
                foreach (var name in headerStructureForLevel.ReferencedNames)
                {
                    fileWriter.WriteField(name);
                }
            }

            foreach (ExportedHeaderItem question in headerStructureForLevel.HeaderItems.Values)
            {
                foreach (var columnName in question.ColumnNames)
                {
                    fileWriter.WriteField(columnName);
                }
            }

            for (int i = 0; i < headerStructureForLevel.LevelScopeVector.Length; i++)
            {
                fileWriter.WriteField(string.Format("{0}{1}", parentId, i + 1));
            }
            fileWriter.NextRecord();
        }

        private IEnumerable<string[]> QueryFromActionTable(InterviewExportedAction? action, Guid questionnaireId,
            long questionnaireVersion)
        {
            Expression<Func<InterviewHistory, bool>> queryActions =
                (i) => i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == questionnaireVersion;

            if (action.HasValue)
            {
                queryActions =
                    (i) =>
                        i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == questionnaireVersion &&
                        i.InterviewActions.Any(a => a.Action == action.Value);
            }

            IEnumerable<InterviewHistory> actions =
                interviewActionsDataStorage.Query(
                    _ =>
                        _.Where(queryActions)
                            .Select(
                                i =>
                                    new InterviewHistory()
                                    {
                                        InterviewId = i.InterviewId,
                                        InterviewActions = i.InterviewActions
                                    }).ToList());

            var result = new List<string[]>();

            foreach (var interviewHistory in actions)
            {
                foreach (var interviewAction in interviewHistory.InterviewActions)
                {
                    var resultRow = new List<string>();
                    resultRow.Add(interviewHistory.InterviewId);
                    resultRow.Add(interviewAction.Action.ToString());
                    resultRow.Add(interviewAction.Originator);
                    resultRow.Add(interviewAction.Role);
                    resultRow.Add(interviewAction.Timestamp.ToString("d", CultureInfo.InvariantCulture));
                    resultRow.Add(interviewAction.Timestamp.ToString("T", CultureInfo.InvariantCulture));
                    result.Add(resultRow.ToArray());
                }
            }
            return result;
        }

        private string[] ExportToTabFile(Guid questionnaireId, long questionnaireVersion, string basePath,
            InterviewExportedAction? action = null)
        {
            var structure = questionnaireExportStructureWriter.AsVersioned()
                .Get(questionnaireId.FormatGuid(), questionnaireVersion);

            if (structure == null)
                return new string[0];

            var result = new List<string>();
            this.transactionManager.GetTransactionManager().BeginQueryTransaction();
            var dataFiles = this.CreateDataFiles(basePath, action, structure, questionnaireId, questionnaireVersion);

            result.AddRange(dataFiles);

            var actionFile = this.CreateFileForInterviewActions(action, basePath, questionnaireId, questionnaireVersion);

            result.Add(actionFile);
            this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
            return result.ToArray();
        }

        private string[] CreateDataFiles(string basePath, InterviewExportedAction? action, QuestionnaireExportStructure structure, Guid questionnaireId, long questionnaireVersion)
        {
            var result = new List<string>();

            foreach (var level in structure.HeaderToLevelMap.Values)
            {
                var dataFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, this.createDataFileName(level.LevelName));
                
                using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(dataFilePath, true))
                using (var tabWriter = this.csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
                {
                    this.CreateHeaderForDataFile(tabWriter, level);
                }

                result.Add(dataFilePath);
            }

            Expression<Func<InterviewExportedDataRecord, bool>> queryData =
               (i) => i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == questionnaireVersion;

            if (action.HasValue)
            {
                queryData =
                    (i) =>
                        i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == questionnaireVersion &&
                        i.LastAction == action.Value;
            }

            List<InterviewExportedDataRecord> interviewDatas = interviewExportedDataStorage.Query(
                _ =>
                    _.Where(queryData)
                        .ToList());

            foreach (var interviewExportedDataRecord in interviewDatas)
            {
                var data = jsonUtils.Deserialize<Dictionary<string, string[]>>(interviewExportedDataRecord.Data);
                foreach (var levelName in data.Keys)
                {
                    var dataFilePath =
                        this.fileSystemAccessor.CombinePath(basePath, this.createDataFileName(levelName));

                    using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(dataFilePath, true))
                    using (var tabWriter = this.csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
                    {
                        foreach (var dataByLevel in data[levelName])
                        {
                            var parsedData = dataByLevel.Split(ExportFileSettings.SeparatorOfExportedDataFile);

                            foreach (var cell in parsedData)
                            {
                                tabWriter.WriteField(cell);
                            }

                            tabWriter.NextRecord();
                        }
                    }
                }
            }
            
            return result.ToArray();
        }

        private string CreateFileForInterviewActions(InterviewExportedAction? action, string basePath, Guid questionnaireId, long questionnaireVersion)
        {
            var actionFilePath =
                fileSystemAccessor.CombinePath(basePath, createDataFileName(sqlDataAccessor.InterviewActionsTableName));

            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(actionFilePath, true))
            using (var tabWriter = this.csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
            {
                this.CreateHeaderForActionFile(tabWriter);

                var dataSet = this.QueryFromActionTable(action, questionnaireId, questionnaireVersion);

                foreach (var dataRow in dataSet)
                {
                    foreach (var cell in dataRow)
                    {
                        tabWriter.WriteField(cell);
                    }

                    tabWriter.NextRecord();
                }
            }
            return actionFilePath;
        }
    }
}
