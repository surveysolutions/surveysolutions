using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlToTabDataExportService : IDataExportService
    {
        private readonly ISqlServiceFactory sqlServiceFactory;
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly string separator;
        private readonly Func<string, string> createDataFileName;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISqlDataAccessor sqlDataAccessor;
        private readonly string parentId = "ParentId";

        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter;

        public SqlToTabDataExportService(IFileSystemAccessor fileSystemAccessor, ISqlServiceFactory sqlServiceFactory,
            ICsvWriterFactory csvWriterFactory, ISqlDataAccessor sqlDataAccessor,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureWriter)
        {
            this.sqlServiceFactory = sqlServiceFactory;
            this.csvWriterFactory = csvWriterFactory;
            this.sqlDataAccessor = sqlDataAccessor;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
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

            return this.ExportToTabFile(questionnaireId, questionnaireVersion,allDataFolderPath, this.fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFileName));
        }

        public string[] GetDataFilesForQuestionnaireByInterviewsInApprovedState(Guid questionnaireId, long questionnaireVersion, string basePath)
        {
            var approvedDataFolderPath = sqlDataAccessor.GetApprovedDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(approvedDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(approvedDataFolderPath);

            fileSystemAccessor.CreateDirectory(approvedDataFolderPath);

            return this.ExportToTabFile(questionnaireId, questionnaireVersion,approvedDataFolderPath, fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFileName),
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

        private IEnumerable<DataRow> QueryRecordsFromTableByInterviewsInApprovedStatus(ISqlService sqlService, string tableName, InterviewExportedAction? action)
        {
            if (!action.HasValue)
                return sqlService.Query<DataRow>(string.Format("select * from [{0}]", tableName));

            return sqlService.Query<DataRow>(string.Format("select [{0}].* from [{1}] join [{0}] "
                + "ON [{1}].[{2}]=[{0}].[{2}] "
                + "where [{1}].[Action] = @interviewAction", tableName, sqlDataAccessor.InterviewActionsTableName,
                sqlDataAccessor.InterviewIdColumnName), new { interviewAction = action.Value.ToString() });
        }

        private IEnumerable<string[]> QueryFromActionTable(ISqlService sqlService, InterviewExportedAction? action)
        {
            IEnumerable<dynamic> queryResult;
            if (!action.HasValue)
                queryResult = sqlService.Query(string.Format("select * from [{0}]", sqlDataAccessor.InterviewActionsTableName));
            else
                queryResult = sqlService.Query(string.Format("select i2.* from [{0}] as i1 join [{0}] as i2 "
                    + "on i1.[{1}]=i2.[{1}] where i1.[Action]=@interviewAction", sqlDataAccessor.InterviewActionsTableName, sqlDataAccessor.InterviewIdColumnName),
                    new { interviewAction = action.Value.ToString() });
            var result = new List<string[]>();

            foreach (var row in queryResult)
            {
                var resultRow = new List<string>();
                foreach (var cell in row)
                {
                    resultRow.Add(cell.Value);
                }
                result.Add(resultRow.ToArray());
            }
            return result;
        }

        private string[] ExportToTabFile(Guid questionnaireId, long questionnaireVersion, string basePath, string dbPath,
            InterviewExportedAction? action = null)
        {
            var structure = questionnaireExportStructureWriter.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);

            if (structure == null)
                return new string[0];

            var result = new List<string>();

            using (var sqlService = sqlServiceFactory.CreateSqlService(dbPath))
            {
                var dataFiles = this.CreateDataFiles(basePath, action, structure, sqlService);

                result.AddRange(dataFiles);

                var actionFile = this.CreateFileForInterviewActions(action, basePath, sqlService);

                result.Add(actionFile);
                
                return result.ToArray();
            }
        }

        private string[] CreateDataFiles(string basePath, InterviewExportedAction? action, QuestionnaireExportStructure structure, ISqlService sqlService)
        {
            var result = new List<string>();

            foreach (var level in structure.HeaderToLevelMap.Values)
            {
                var dataFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, this.createDataFileName(level.LevelName));

                this.WriteDataFileForInterivewLvel(action, dataFilePath, level, sqlService);

                result.Add(dataFilePath);
            }

            return result.ToArray();
        }

        private string CreateFileForInterviewActions(InterviewExportedAction? action, string basePath, ISqlService sqlService)
        {
            var actionFilePath =
                fileSystemAccessor.CombinePath(basePath, createDataFileName(sqlDataAccessor.InterviewActionsTableName));

            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(actionFilePath, true))
            using (var tabWriter = this.csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
            {
                this.CreateHeaderForActionFile(tabWriter);

                var dataSet = this.QueryFromActionTable(sqlService, action);

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

        private void WriteDataFileForInterivewLvel(InterviewExportedAction? action, string dataFilePath, HeaderStructureForLevel level,
            ISqlService sqlService)
        {
            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(dataFilePath, true))
            using (var tabWriter = this.csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
            {
                this.CreateHeaderForDataFile(tabWriter, level);

                var dataSet = this.QueryRecordsFromTableByInterviewsInApprovedStatus(sqlService, level.LevelName, action);

                foreach (var dataRow in dataSet)
                {
                    var otherRecords = this.ParseByteArray(dataRow.Data);
                    foreach (var otherRecord in otherRecords)
                    {
                        tabWriter.WriteField(otherRecord ?? string.Empty);
                    }

                    tabWriter.NextRecord();
                }
            }
        }

        private string[] ParseByteArray(byte[] bytes)
        {
            return Encoding.Unicode.GetString(bytes).Split(ExportFileSettings.SeparatorOfExportedDataFile);
        }
    }
}
