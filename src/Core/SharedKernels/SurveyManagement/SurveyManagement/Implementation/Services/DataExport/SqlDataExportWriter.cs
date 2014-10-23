using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlDataExportWriter : IDataExportWriter
    {
        private const string text = "ntext";
        private const string numeric = "money";
        private const string nvarchar = "nvarchar(512)";
        private const string dataFile = "data.sdf";
        private const string interviewActions = "interview_actions";
        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";
        private const string parentId = "ParentId";
        
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private ISqlServiceFactory sqlServiceFactory;

        private readonly QuestionType[] numericQuestionTypes = new[] { QuestionType.SingleOption, QuestionType.MultyOption, QuestionType.Numeric };

        public SqlDataExportWriter(ICsvWriterFactory csvWriterFactory, IFileSystemAccessor fileSystemAccessor, ISqlServiceFactory sqlServiceFactory)
        {
            this.csvWriterFactory = csvWriterFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.sqlServiceFactory = sqlServiceFactory;
        }

        private void AddRecordsImpl(InterviewDataExportLevelView items, ISqlService sqlService)
        {
            var deletePreviousRecordsCommand = string.Format("DELETE FROM \"{0}\" WHERE {1} = '{2}';", items.LevelName,
                items.LevelVector.Length == 0 ? "Id" : string.Format("{0}{1}", parentId, items.LevelVector.Length), items.InterviewId);
            var commandsToExecute = new List<string>();

            commandsToExecute.Add(deletePreviousRecordsCommand);

            foreach (var item in items.Records)
            {
                var insertCommand = string.Format("insert into \"{0}\" values (", items.LevelName);
                insertCommand = insertCommand + (items.LevelVector.Length == 0 ? string.Format("'{0}'", item.RecordId) : item.RecordId);

                foreach (var referenceValue in item.ReferenceValues)
                {
                    insertCommand = insertCommand + ",'" + QuoteString(referenceValue) + "'";
                }

                foreach (var exportedQuestion in item.Questions)
                {
                    foreach (string itemValue in exportedQuestion.Answers)
                    {
                        insertCommand = insertCommand + "," + (string.IsNullOrEmpty(itemValue)
                            ? "null"
                            : (numericQuestionTypes.Contains(exportedQuestion.QuestionType)
                                ? itemValue
                                : string.Format("'{0}'", QuoteString(itemValue))));
                    }
                }

                for (int i = 0; i < item.ParentRecordIds.Length; i++)
                {
                    insertCommand = insertCommand + "," +
                        (item.ParentRecordIds.Length - 1 == i
                            ? string.Format("'{0}'", item.ParentRecordIds[i])
                            : item.ParentRecordIds[i]);
                }
                insertCommand = insertCommand + ");";
                commandsToExecute.Add(insertCommand);
            }

            sqlService.ExecuteCommands(commandsToExecute);

        }

        private string QuoteString(string val)
        {
            return val.Replace("'", "''");
        }

        public string[] GetAllDataFiles(string basePath)
        {
            var allDataFolderPath = GetAllDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(allDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(allDataFolderPath);

            fileSystemAccessor.CreateDirectory(allDataFolderPath);

            return this.ExportToCSVFile(allDataFolderPath, this.fileSystemAccessor.CombinePath(basePath, dataFile), (tableName, column) => string.Format("select * from \"{0}\"", tableName));
        }

        public string[] GetApprovedDataFiles(string basePath)
        {
            var approvedDataFolderPath = GetApprovedDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(approvedDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(approvedDataFolderPath);

            fileSystemAccessor.CreateDirectory(approvedDataFolderPath);

            return ExportToCSVFile(approvedDataFolderPath, fileSystemAccessor.CombinePath(basePath, dataFile),
                CreateQueryStringForApprovedInterviewsByTableName);
        }

        public void DeleteInterviewRecords(string basePath, Guid interviewId)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, dataFile)))
            {
                var deleteInterviewRecords = new List<string>();
                var tableNames = this.GetListofTables(sqlService);
                foreach (var tableName in tableNames)
                {
                    var columnNames = this.GetListOfColumns(sqlService, tableName);
                    deleteInterviewRecords.Add(string.Format("DELETE FROM \"{0}\" WHERE {1} = '{2}';", tableName,
                        columnNames.Any(c => c.StartsWith(parentId)) ? columnNames.Last() : "Id", interviewId.FormatGuid()));
                }
                sqlService.ExecuteCommands(deleteInterviewRecords);
            }
        }

        private IEnumerable<string> GetListofTables(ISqlService sqlService)
        {
            return sqlService.ExecuteReader("select table_name from information_schema.tables where TABLE_TYPE = 'TABLE'").Select(table=>table[0].ToString());
        }

        private IEnumerable<string> GetListOfColumns(ISqlService sqlService, string tableName)
        {
            return sqlService.ExecuteReader("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'").Select(table => table[0].ToString());
        } 

        private string GetAllDataFolder(string basePath)
        {
            return fileSystemAccessor.CombinePath(basePath, allDataFolder);
        }

        private string GetApprovedDataFolder(string basePath)
        {
            return fileSystemAccessor.CombinePath(basePath, approvedDataFolder);
        }

        private string CreateQueryStringForApprovedInterviewsByTableName(string tableName, IEnumerable<string> columnNames)
        {
            var filterByAction = InterviewExportedAction.ApproveByHeadquarter;
            if (tableName == interviewActions)
                return string.Format("select i2.* from {0} as i1 join {0} as i2 "
                    + "on i1.Id=i2.Id where i1.Action='{1}'", tableName, filterByAction);

            if (!columnNames.Any(name => name.StartsWith(parentId)))
                return string.Format("select \"{0}\".* from \"{1}\" join \"{0}\" "
                    + "ON \"{1}\".Id=\"{0}\".Id "
                    + "where \"{1}\".Action='{2}'", tableName, interviewActions, filterByAction);

            return string.Format("select \"{0}\".* from \"{1}\" join \"{0}\" "
                + "ON \"{1}\".Id=\"{0}\".{3} "
                + "where \"{1}\".Action='{2}'", tableName, interviewActions, filterByAction,
                columnNames.Last());
        }

        private string[] ExportToCSVFile(string basePath, string dbPath, Func<string, IEnumerable<string>, string> createSqlQueryFormat)
        {
            var result = new List<string>();
            using (var sqlService = sqlServiceFactory.CreateSqlService(dbPath))
            {
                var tableNames = this.GetListofTables(sqlService);

                foreach (var tableName in tableNames)
                {
                    var csvFilePath =
                        fileSystemAccessor.CombinePath(basePath, tableName + ".tab");

                    var columnNames = GetListOfColumns(sqlService, tableName).ToArray();
                    using (var fileStream = fileSystemAccessor.OpenOrCreateFile(csvFilePath, true))
                    using (var csv = csvWriterFactory.OpenCsvWriter(fileStream, "\t "))
                    {
                        foreach (var columnName in columnNames)
                        {
                            csv.WriteField(columnName);
                        }

                        csv.NextRecord();
                        var dataSet = sqlService.ExecuteReader(createSqlQueryFormat(tableName, columnNames));
                        foreach (var dataRow in dataSet)
                        {
                            for (int i = 0; i < dataRow.Length; i++)
                            {
                                csv.WriteField(dataRow[i]);
                            }

                            csv.NextRecord();
                        }

                        result.Add(csvFilePath);
                    }
                }
            }
            return result.ToArray();
        }

        private string CreaActionRecordInsertCommand(InterviewActionExportView action)
        {
            var insertActionRecord = string.Format("insert into \"{0}\" values (", interviewActions);
            insertActionRecord = insertActionRecord + string.Format("'{0}'", action.InterviewId);
            insertActionRecord = insertActionRecord + string.Format(", '{0}'", action.Action);
            insertActionRecord = insertActionRecord + string.Format(", '{0}'", action.Originator);
            insertActionRecord = insertActionRecord + string.Format(", '{0}'", action.Role);
            insertActionRecord = insertActionRecord +
                string.Format(", '{0}'", action.Timestamp.ToString("d", CultureInfo.InvariantCulture));
            insertActionRecord = insertActionRecord +
                string.Format(", '{0}'", action.Timestamp.ToString("T", CultureInfo.InvariantCulture));
            insertActionRecord = insertActionRecord + ");";
            return insertActionRecord;
        }
        public void AddActionRecord(InterviewActionExportView action, string basePath)
        {
            var insertActionRecord = CreaActionRecordInsertCommand(action);

            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, dataFile)))
            {
                sqlService.ExecuteCommand(insertActionRecord);
            }

            var folderPath = GetFolderPath(basePath);
            fileSystemAccessor.DeleteDirectory(this.GetAllDataFolder(folderPath));

            if (action.Action == InterviewExportedAction.ApproveByHeadquarter)
                fileSystemAccessor.DeleteDirectory(this.GetApprovedDataFolder(folderPath));
        }


        public void BatchInsert(string basePath, IEnumerable<InterviewDataExportView> interviewDatas, IEnumerable<InterviewActionExportView> interviewActionRecords)
        {
            var commands = new List<string>();

            foreach (var interviewActionRecord in interviewActionRecords)
            {
                commands.Add(this.CreaActionRecordInsertCommand(interviewActionRecord));
            }

            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, dataFile)))
            {
                foreach (var interviewDataExportView in interviewDatas)
                {
                    foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
                    {
                        AddRecordsImpl(interviewDataExportLevelView, sqlService);
                    }
                }
                sqlService.ExecuteCommands(commands);
            }
        }

        public void AddOrUpdateInterviewRecords(InterviewDataExportView items, string basePath)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath,dataFile)))
            {
                foreach (var interviewDataExportLevelView in items.Levels)
                {
                    AddRecordsImpl(interviewDataExportLevelView, sqlService);
                }
            }
        }


        public void CreateStructure(QuestionnaireExportStructure header, string basePath)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, dataFile)))
            {
                foreach (var headerStructureForLevel in header.HeaderToLevelMap.Values)
                {
                    this.CreateHeader(headerStructureForLevel, sqlService);
                }
                this.CreateHeaderForActionFile(sqlService);
            }
        }

        private string GetFolderPath(string dbPath)
        {
            return dbPath.Substring(0, dbPath.Length - fileSystemAccessor.GetFileName(dbPath).Length);
        }

        private void CreateHeader(HeaderStructureForLevel header, ISqlService sqlService)
        {
            var createLevelTable = string.Format("create table \"{0}\" (", header.LevelName);

            createLevelTable = createLevelTable +
                string.Format("{0} {1}", header.LevelIdColumnName, header.LevelScopeVector.Length == 0 ? nvarchar : numeric);

            if (header.IsTextListScope)
            {
                foreach (var name in header.ReferencedNames)
                {
                    createLevelTable = createLevelTable + ", " + string.Format("[{0}] {1}", name, text);
                }
            }

            foreach (ExportedHeaderItem question in header.HeaderItems.Values)
            {
                var columnType = numericQuestionTypes.Contains(question.QuestionType) ? numeric : text;
                foreach (var columnName in question.ColumnNames)

                {
                    createLevelTable = createLevelTable + ", " +
                        string.Format("[{0}] {1}", columnName, columnType);
                }
            }

            for (int i = 0; i < header.LevelScopeVector.Length; i++)
            {
                createLevelTable = createLevelTable + ", " +
                    string.Format("{0} {1}", string.Format("{0}{1}", parentId, i + 1),
                        i == header.LevelScopeVector.Length - 1 ? nvarchar : numeric);
            }

            createLevelTable = createLevelTable + ");";



            var createLevelTableIndex = string.Format("CREATE INDEX idx{0} ON \"{1}\"({2});",
                header.LevelScopeVector.Length == 0 ? "Main" : header.LevelName, header.LevelName,
                header.LevelScopeVector.Length == 0
                    ? header.LevelIdColumnName
                    : string.Format("{0}{1}", parentId, header.LevelScopeVector.Length));

            sqlService.ExecuteCommands(new[] { createLevelTable, createLevelTableIndex });

        }

        private void CreateHeaderForActionFile(ISqlService sqlService)
        {
            var createHeaderTabaleCommand = string.Format("create table {0} (", interviewActions);
            createHeaderTabaleCommand += "Id " + nvarchar;
            createHeaderTabaleCommand += ", Action " + nvarchar;
            createHeaderTabaleCommand += ", Originator " + nvarchar;
            createHeaderTabaleCommand += ", Role " + nvarchar;
            createHeaderTabaleCommand += ", Date " + nvarchar;
            createHeaderTabaleCommand += ", Time " + nvarchar;
            createHeaderTabaleCommand = createHeaderTabaleCommand + ");";

            sqlService.ExecuteCommand(createHeaderTabaleCommand);
        }
    }
}
