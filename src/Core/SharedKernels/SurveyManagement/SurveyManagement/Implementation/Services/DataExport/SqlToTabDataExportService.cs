﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using StatData.Core;
using StatData.Writers;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;


namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlToTabDataExportService : IDataExportService
    {
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly string separator;
        private readonly Func<string, string> createDataFileName;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IExportedDataAccessor exportedDataAccessor;
        private readonly string parentId = "ParentId";
        private readonly IQueryableReadSideRepositoryReader<InterviewExportedDataRecord> interviewExportedDataStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewActionsDataStorage;
        private readonly ILogger logger;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IJsonUtils jsonUtils;
        private readonly ITabFileReader tabReader;
        private readonly IDatasetWriterFactory datasetWriterFactory;

        public SqlToTabDataExportService(
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriterFactory csvWriterFactory, 
            IExportedDataAccessor exportedDataAccessor,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            IQueryableReadSideRepositoryReader<InterviewExportedDataRecord> interviewExportedDataStorage,
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewActionsDataStorage, 
            IJsonUtils jsonUtils, 
            ITransactionManagerProvider transactionManager,
            ILogger logger,
            ITabFileReader tabReader,
            IDatasetWriterFactory datasetWriterFactory)
        {
            this.csvWriterFactory = csvWriterFactory;
            this.exportedDataAccessor = exportedDataAccessor;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.interviewExportedDataStorage = interviewExportedDataStorage;
            this.interviewActionsDataStorage = interviewActionsDataStorage;
            this.jsonUtils = jsonUtils;
            this.transactionManager = transactionManager;
            this.createDataFileName = ExportFileSettings.GetContentFileName;
            this.separator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;

            this.tabReader = tabReader;
            this.datasetWriterFactory = datasetWriterFactory;
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string targetFolder)
        {
            var structure = 
                this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                 questionnaireExportStructureWriter.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion));

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

        private string DataFileNameExtension { get { return ".tab"; } }
        private string StataFileNameExtension { get { return ".dta"; } }
        private string SpssFileNameExtension { get { return ".sav"; } }

        public string[] GetDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            var allDataFolderPath = exportedDataAccessor.GetAllDataFolder(basePath);
            
            if (!fileSystemAccessor.IsDirectoryExists(allDataFolderPath))
            {
                fileSystemAccessor.CreateDirectory(allDataFolderPath);

                this.ExportToTabFile(questionnaireId, questionnaireVersion, allDataFolderPath);
                
            }

            return fileSystemAccessor.GetFilesInDirectory(allDataFolderPath)
                .Where(fileName => fileName.EndsWith(DataFileNameExtension)).ToArray();
        }

        public string[] CreateAndGetStataDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            var dataFiles = GetDataFilesForQuestionnaire(questionnaireId, questionnaireVersion, basePath); 
            return CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, basePath, ExportDataType.Stata, dataFiles);
        }
        
        public string[] CreateAndGetSpssDataFilesForQuestionnaire(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            var dataFiles = GetDataFilesForQuestionnaire(questionnaireId, questionnaireVersion, basePath);
            return CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, basePath, ExportDataType.Spss, dataFiles);
        }

        private void CollectLabels(QuestionnaireExportStructure structure, out Dictionary<string, string> labels, out Dictionary<string, Dictionary<double, string>> varValueLabels)
        {
            labels = new Dictionary<string, string>();
            varValueLabels = new Dictionary<string, Dictionary<double,string>>();

            foreach (var headerStructureForLevel in structure.HeaderToLevelMap.Values)
            {
                foreach (ExportedHeaderItem headerItem in headerStructureForLevel.HeaderItems.Values)
                {
                    bool hasLabels = headerItem.Labels != null && headerItem.Labels.Count > 0;
                    
                    if (hasLabels)
                    {
                        string labelName = headerItem.VariableName;
                        if (!varValueLabels.ContainsKey(labelName))
                        {
                            var items = headerItem.Labels.Values.ToDictionary(item => Double.Parse(item.Caption),item => item.Title ?? string.Empty);
                            varValueLabels.Add(labelName, items);
                        }
                    }

                    for (int i = 0; i < headerItem.ColumnNames.Length; i++)
                    {
                        if (!labels.ContainsKey(headerItem.ColumnNames[i]))
                            labels.Add(headerItem.ColumnNames[i], headerItem.Titles[i] ?? string.Empty);
                    }
                }

                if (headerStructureForLevel.LevelLabels == null) continue;
                
                var levelLabelName = headerStructureForLevel.LevelIdColumnName;
                if (varValueLabels.ContainsKey(levelLabelName)) continue;
                    
                var labelItems = headerStructureForLevel.LevelLabels.ToDictionary(item => Double.Parse(item.Caption), item => item.Title ?? String.Empty);
                varValueLabels.Add(levelLabelName, labelItems);
            }
        }

        private string[] CreateAndGetExportDataFiles(Guid questionnaireId, long questionnaireVersion, string basePath, ExportDataType exportType, string[] dataFiles)
        {
            string currentDataInfo = string.Empty;
            try
            {
                var structure = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() =>
                    questionnaireExportStructureWriter.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion));

                if (structure == null)
                    return new string[0];

                Dictionary<string, string> varLabels;
                Dictionary<string, Dictionary<double, string>> varValueLabels;

                CollectLabels(structure, out varLabels, out varValueLabels);

                var result = new List<string>();
                string fileExtention = exportType == ExportDataType.Stata
                    ? StataFileNameExtension
                    : SpssFileNameExtension;
                var writer = datasetWriterFactory.CreateDatasetWriter(exportType);
                foreach (var tabFile in dataFiles)
                {
                    string dataFile = fileSystemAccessor.ChangeExtension(tabFile, fileExtention);
                    var meta = tabReader.GetMetaFromTabFile(tabFile);
                    
                    UpdateMetaWithLabels(meta, varLabels, varValueLabels);
                    currentDataInfo = string.Format("filename: {0}", tabFile);
                    var data = tabReader.GetDataFromTabFile(tabFile);
                    writer.WriteToFile(dataFile, meta, data);
                    result.Add(dataFile);
                }

                return result.ToArray();

            }
            catch (Exception exc)
            {
                logger.Error(string.Format("Error on data export (questionnaireId:{0}, questionnaireVersion:{1}): ", questionnaireId, questionnaireVersion), exc);
                logger.Error(currentDataInfo);
            }

            return new string[0];
        }

        private static void UpdateMetaWithLabels(IDatasetMeta meta, Dictionary<string, string> varLabels, Dictionary<string, Dictionary<double, string>> varValueLabels)
        {
            foreach (var datasetVariable in meta.Variables)
            {
                if (varLabels.ContainsKey(datasetVariable.VarName))
                    datasetVariable.VarLabel = varLabels[datasetVariable.VarName];

                if (varValueLabels.ContainsKey(datasetVariable.VarName))
                {
                    var valueSet = new StatData.Core.ValueSet();
                    foreach (var variable in varValueLabels[datasetVariable.VarName])
                    {
                        valueSet.Add(variable.Key, variable.Value);
                    }

                    meta.AssociateValueSet(datasetVariable.VarName, valueSet);
                }
            }
        }

        public string[] CreateAndGetStataDataFilesForQuestionnaireInApprovedState(Guid questionnaireId, long questionnaireVersion,
            string basePath)
        {
            var dataFiles = GetDataFilesForQuestionnaireByInterviewsInApprovedState(questionnaireId, questionnaireVersion, basePath);
            return CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, basePath, ExportDataType.Stata, dataFiles);
        }

        public string[] CreateAndGetSpssDataFilesForQuestionnaireInApprovedState(Guid questionnaireId, long questionnaireVersion,
            string basePath)
        {
            var dataFiles = GetDataFilesForQuestionnaireByInterviewsInApprovedState(questionnaireId, questionnaireVersion, basePath);
            return CreateAndGetExportDataFiles(questionnaireId, questionnaireVersion, basePath, ExportDataType.Spss, dataFiles);
        }

        public string[] GetDataFilesForQuestionnaireByInterviewsInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            var approvedDataFolderPath = exportedDataAccessor.GetApprovedDataFolder(basePath);

            if (!fileSystemAccessor.IsDirectoryExists(approvedDataFolderPath))
            {
                fileSystemAccessor.CreateDirectory(approvedDataFolderPath);
                
                this.ExportToTabFile(questionnaireId, questionnaireVersion, approvedDataFolderPath,
                    InterviewExportedAction.ApprovedByHeadquarter);
            }

            return fileSystemAccessor.GetFilesInDirectory(approvedDataFolderPath)
                .Where(fileName => fileName.EndsWith(DataFileNameExtension)).ToArray();
        }

        private void CreateHeaderForActionFile(ICsvWriterService fileWriter)
        {
            foreach (var actionFileColumn in exportedDataAccessor.ActionFileColumns)
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
            Expression<Func<InterviewStatuses, bool>> queryActions =
                (i) =>
                    i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == questionnaireVersion &&
                    i.InterviewCommentedStatuses.Any(a => a.Status != InterviewExportedAction.Deleted);

            if (action.HasValue)
            {
                queryActions =
                    (i) =>
                        i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == questionnaireVersion &&
                        i.InterviewCommentedStatuses.Any(a => a.Status == action.Value);
            }

            IEnumerable<InterviewStatuses> actions =
                interviewActionsDataStorage.Query(
                    _ =>
                        _.Where(queryActions)
                            .Select(
                                i =>
                                    new InterviewStatuses()
                                    {
                                        InterviewId = i.InterviewId,
                                        InterviewCommentedStatuses = i.InterviewCommentedStatuses
                                    }).ToList());

            var result = new List<string[]>();

            foreach (var interviewHistory in actions)
            {
                foreach (var interviewAction in interviewHistory.InterviewCommentedStatuses)
                {
                    var resultRow = new List<string>();
                    resultRow.Add(interviewHistory.InterviewId);
                    resultRow.Add(interviewAction.Status.ToString());
                    resultRow.Add(interviewAction.StatusChangeOriginatorName);
                    resultRow.Add(/*interviewAction.Role*/"");
                    resultRow.Add(interviewAction.Timestamp.ToString("d", CultureInfo.InvariantCulture));
                    resultRow.Add(interviewAction.Timestamp.ToString("T", CultureInfo.InvariantCulture));
                    result.Add(resultRow.ToArray());
                }
            }
            return result;
        }

        void ExportToTabFile(Guid questionnaireId, long questionnaireVersion, string basePath,
            InterviewExportedAction? action = null)
        {
            this.transactionManager.GetTransactionManager().BeginQueryTransaction();
            try
            {
                var structure = questionnaireExportStructureWriter.AsVersioned()
                    .Get(questionnaireId.FormatGuid(), questionnaireVersion);

                if (structure == null)
                    return;
                this.CreateDataFiles(basePath, action, structure, questionnaireId, questionnaireVersion);
                this.CreateFileForInterviewActions(action, basePath, questionnaireId, questionnaireVersion);
            }
            finally
            {
                this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
            }
        }

        private void CreateDataFiles(string basePath, InterviewExportedAction? action, QuestionnaireExportStructure structure, Guid questionnaireId, long questionnaireVersion)
        {
            
            foreach (var level in structure.HeaderToLevelMap.Values)
            {
                var dataFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, this.createDataFileName(level.LevelName));

                
                using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(dataFilePath, true))
                using (var tabWriter = this.csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
                {
                    this.CreateHeaderForDataFile(tabWriter, level);
                }
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
            
        }

        private void CreateFileForInterviewActions(InterviewExportedAction? action, string basePath, Guid questionnaireId, long questionnaireVersion)
        {
            var actionFilePath =
                fileSystemAccessor.CombinePath(basePath, createDataFileName(exportedDataAccessor.InterviewActionsTableName));

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
        }
    }
}
