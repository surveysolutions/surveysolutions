using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
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
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlCeDataExportWriter : IDataExportWriter
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

        private readonly QuestionType[] numericQuestionTypes = new[] { QuestionType.SingleOption, QuestionType.MultyOption, QuestionType.Numeric };

        public SqlCeDataExportWriter(ICsvWriterFactory csvWriterFactory, IFileSystemAccessor fileSystemAccessor)
        {
            this.csvWriterFactory = csvWriterFactory;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void AddRecords(InterviewDataExportLevelView items, string basePath)
        {
            var commandText = string.Format("DELETE FROM \"{0}\" WHERE {1} = '{2}';", items.LevelName,
                items.LevelVector.Length == 0 ? "Id" : string.Format("{0}{1}", parentId, items.LevelVector.Length), items.InterviewId);

            ExecuteSqlLite(basePath, (db) =>
            {
                using (var createTableCommand = db.CreateCommand())
                {
                    createTableCommand.CommandText = commandText;
                    createTableCommand.ExecuteNonQuery();
                }

                foreach (var item in items.Records)
                {
                    commandText = string.Format("insert into \"{0}\" values (", items.LevelName);
                    commandText = commandText + (items.LevelVector.Length == 0 ? string.Format("'{0}'", item.RecordId) : item.RecordId);

                    foreach (var referenceValue in item.ReferenceValues)
                    {
                        commandText = commandText + ",'" + QuoteString(referenceValue) + "'";
                    }

                    foreach (var exportedQuestion in item.Questions)
                    {
                        foreach (string itemValue in exportedQuestion.Answers)
                        {
                            commandText = commandText + "," + (string.IsNullOrEmpty(itemValue)
                                ? "null"
                                : (numericQuestionTypes.Contains(exportedQuestion.QuestionType)
                                    ? itemValue
                                    : string.Format("'{0}'", QuoteString(itemValue))));
                        }
                    }

                    for (int i = 0; i < item.ParentRecordIds.Length; i++)
                    {
                        commandText = commandText + "," +
                            (item.ParentRecordIds.Length - 1 == i
                                ? string.Format("'{0}'", item.ParentRecordIds[i])
                                : item.ParentRecordIds[i]);
                    }
                    commandText = commandText + ");";

                    using (var createTableCommand = db.CreateCommand())
                    {
                        createTableCommand.CommandText = commandText;
                        createTableCommand.ExecuteNonQuery();
                    }
                }
            });
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
            this.ExecuteSqlLite(fileSystemAccessor.CombinePath(basePath, dataFile), (db) =>
            {
                var tableNames = this.GetListofTables(db);
                foreach (var tableName in tableNames)
                {
                    var columnNames = this.GetListOfColumns(db, tableName);
                    using (var commamd = db.CreateCommand())
                    {
                        commamd.CommandText = string.Format("DELETE FROM \"{0}\" WHERE {1} = '{2}';", tableName,
                            columnNames.Any(c => c.StartsWith(parentId)) ? columnNames.Last() : "Id", interviewId.FormatGuid());
                        commamd.ExecuteNonQuery();
                    }
                }
            });
        }

        private IEnumerable<string> GetListofTables(DbConnection db)
        {
            var tableNames = new List<string>();

            using (var getListOfTables = db.CreateCommand())
            {
                getListOfTables.CommandText = "select table_name from information_schema.tables where TABLE_TYPE = 'TABLE'";
                var tableNamesReader = getListOfTables.ExecuteReader();
                while (tableNamesReader.Read())
                {
                    tableNames.Add(Convert.ToString(tableNamesReader.GetString(0)));
                }
            }
            return tableNames;
        }

        private IEnumerable<string> GetListOfColumns(DbConnection db, string tableName)
        {
             var columnNames = new List<string>();
            using (var columnCommand = db.CreateCommand())
            {
                columnCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
                var columnReader = columnCommand.ExecuteReader();
                while (columnReader.Read())
                {
                    var columnName = columnReader.GetString(0);
                    columnNames.Add(columnReader.GetString(0));
                }
            }
            return columnNames;
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

        private string[] ExportToCSVFile(string basePath, string dbPath, Func<string,IEnumerable<string>, string> createSqlQueryFormat)
        {
            var result = new List<string>();
            ExecuteSqlLite(dbPath, (db) =>
            {
                var tableNames = this.GetListofTables(db);

                foreach (var tableName in tableNames)
                {
                    var csvFilePath =
                        fileSystemAccessor.CombinePath(basePath, tableName + ".tab");
                    
                    var columnNames = GetListOfColumns(db, tableName);
                    using (var fileStream = fileSystemAccessor.OpenOrCreateFile(csvFilePath, true))
                    using (var csv = csvWriterFactory.OpenCsvWriter(fileStream, "\t "))
                    {
                        foreach (var columnName in columnNames)
                        {
                            csv.WriteField(columnName);
                        }

                        csv.NextRecord();

                        using (var command = db.CreateCommand())
                        {
                            command.CommandText = createSqlQueryFormat(tableName, columnNames);

                            var dr = command.ExecuteReader();

                            while (dr.Read())
                            {
                                for (int i = 0; i < dr.FieldCount; i++)
                                {
                                    csv.WriteField(dr[i]);
                                }

                                csv.NextRecord();
                            }
                        }

                        result.Add(csvFilePath);
                    }
                }
            });
            return result.ToArray();
        }

        public void AddActionRecord(InterviewActionExportView action, string basePath)
        {
            var commandText = string.Format("insert into \"{0}\" values (", interviewActions);
            commandText = commandText + string.Format("'{0}'", action.InterviewId);
            commandText = commandText + string.Format(", '{0}'", action.Action);
            commandText = commandText + string.Format(", '{0}'", action.Originator);
            commandText = commandText + string.Format(", '{0}'", action.Role);
            commandText = commandText +
                string.Format(", '{0}'", action.Timestamp.ToString("d", CultureInfo.InvariantCulture));
            commandText = commandText +
                string.Format(", '{0}'", action.Timestamp.ToString("T", CultureInfo.InvariantCulture));
            commandText = commandText + ");";

            ExecuteSqlLite(basePath, (db) =>
            {
                using (var createTableCommand = db.CreateCommand())
                {
                    createTableCommand.CommandText = commandText;
                    createTableCommand.ExecuteNonQuery();
                }
            });

            var folderPath = basePath.Substring(0, basePath.Length - fileSystemAccessor.GetFileName(basePath).Length);

            fileSystemAccessor.DeleteDirectory(this.GetAllDataFolder(folderPath));
            if (action.Action == InterviewExportedAction.ApproveByHeadquarter)
                fileSystemAccessor.DeleteDirectory(this.GetAllDataFolder(folderPath));
        }

        public void CreateHeader(HeaderStructureForLevel header, string basePath)
        {
            CreateDBIfAbent(basePath);
            var commandText = string.Format("create table \"{0}\" (", header.LevelName);

            commandText = commandText +
                string.Format("{0} {1}", header.LevelIdColumnName, header.LevelScopeVector.Length == 0 ? nvarchar : numeric);

            if (header.IsTextListScope)
            {
                foreach (var name in header.ReferencedNames)
                {
                    commandText = commandText + ", " + string.Format("[{0}] {1}", name, text);
                }
            }

            foreach (ExportedHeaderItem question in header.HeaderItems.Values)
            {
                foreach (var columnName in question.ColumnNames)

                {
                    commandText = commandText + ", " + string.Format("[{0}] {1}", columnName, numericQuestionTypes.Contains(question.QuestionType) ? numeric : text);
                }
            }

            for (int i = 0; i < header.LevelScopeVector.Length; i++)
            {
                commandText = commandText + ", " +
                    string.Format("{0} {1}", string.Format("{0}{1}",parentId, i + 1), i == header.LevelScopeVector.Length - 1 ? nvarchar : numeric);
            }

            commandText = commandText + ")";

            ExecuteSqlLite(basePath, (db) =>
            {
                using (var createTableCommand = db.CreateCommand())
                {
                    createTableCommand.CommandText = commandText;
                    createTableCommand.ExecuteNonQuery();
                }

                commandText = string.Format("CREATE INDEX idx{0} ON \"{1}\"({2});",
                header.LevelScopeVector.Length == 0 ? "Main" : header.LevelName, header.LevelName,
                header.LevelScopeVector.Length == 0
                    ? header.LevelIdColumnName
                    : string.Format("{0}{1}",parentId, header.LevelScopeVector.Length));

                using (var createTableCommand = db.CreateCommand())
                {
                    createTableCommand.CommandText = commandText;
                    createTableCommand.ExecuteNonQuery();
                }
            });
        }

        private void ExecuteSqlLite(string dbPath, Action<DbConnection> action)
        {
            using (var db = new SqlCeConnection("Data Source=\"" + dbPath + "\";"))
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        db.Open();
                        action(db);

                        scope.Complete();
                    }
                }
                catch(SqlCeException)
                {
                    if (db.State != System.Data.ConnectionState.Closed)
                    {
                        db.Close();
                    }
                    throw;
                }
            }
        }

        private void CreateDBIfAbent(string dbPath)
        {
            if(fileSystemAccessor.IsFileExists(dbPath))
                return;

            string connectionString = "DataSource=\"" + dbPath + "\"";
            using (var en = new SqlCeEngine(connectionString))
            {
                en.CreateDatabase();
            }
        }
        public void CreateHeaderForActionFile(string basePath)
        {
            CreateDBIfAbent(basePath);
            var commandText = string.Format("create table {0} (", interviewActions);
            commandText += "Id " + nvarchar;
            commandText += ", Action " + nvarchar;
            commandText += ", Originator " + nvarchar;
            commandText += ", Role " + nvarchar;
            commandText += ", Date " + nvarchar;
            commandText += ", Time " + nvarchar;
            commandText = commandText + ")";

            ExecuteSqlLite(basePath, (db) =>
            {
                using (var createTableCommand = db.CreateCommand())
                {
                    createTableCommand.CommandText = commandText;
                    createTableCommand.ExecuteNonQuery();
                }
            });
        }

        public string GetInterviewExportedDataFileName(string levelName)
        {
            return dataFile;
        }

        public string GetInterviewActionFileName()
        {
            return dataFile;
        }
    }
}
