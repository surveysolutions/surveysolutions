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
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlDataExportWriter : BaseSqlService, IDataExportWriter
    {
        private const string text = "ntext";
        private const string numeric = "money";
        private const string nvarchar = "nvarchar(512)";
        
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

        private void AddRecordsToLevel(InterviewDataExportLevelView items, ISqlService sqlService)
        {
            var isMainLevel = items.LevelVector.Length == 0;
            sqlService.ExecuteCommand(
                string.Format("DELETE FROM \"{0}\" WHERE {1} = @interviewId;", items.LevelName,
                    isMainLevel ? "Id" : string.Format("{0}{1}", parentId, items.LevelVector.Length)),
                new { interviewId = items.InterviewId });

            foreach (var item in items.Records)
            {
                var insertCommand = string.Format("insert into \"{0}\" values (", items.LevelName);
                insertCommand = insertCommand + (isMainLevel ? string.Format("'{0}'", item.RecordId) : item.RecordId);

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
                sqlService.ExecuteCommand(insertCommand);
            }
        }

        private string QuoteString(string val)
        {
            return val.Replace("'", "''");
        }

        public void DeleteInterviewRecords(string basePath, Guid interviewId)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, dataFile)))
            {
                var tableNames = this.GetListofTables(sqlService);
                foreach (var tableName in tableNames)
                {
                    var columnNames = this.GetListOfColumns(sqlService, tableName);

                    sqlService.ExecuteCommand(string.Format("DELETE FROM \"{0}\" WHERE {1} = @interviewId;", tableName,
                        columnNames.Any(c => c.StartsWith(parentId)) ? columnNames.Last() : "Id"),
                        new { interviewId = interviewId.FormatGuid() });
                }
            }
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

          /*  var folderPath = GetFolderPath(basePath);
            fileSystemAccessor.DeleteDirectory(this.GetAllDataFolder(folderPath));

            if (action.Action == InterviewExportedAction.ApproveByHeadquarter)
                fileSystemAccessor.DeleteDirectory(this.GetApprovedDataFolder(folderPath));*/
        }


        public void BatchInsert(string basePath, IEnumerable<InterviewDataExportView> interviewDatas,
            IEnumerable<InterviewActionExportView> interviewActionRecords)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, dataFile)))
            {
                foreach (var interviewActionRecord in interviewActionRecords)
                {
                    sqlService.ExecuteCommand(this.CreaActionRecordInsertCommand(interviewActionRecord));
                }

                foreach (var interviewDataExportView in interviewDatas)
                {
                    foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
                    {
                        this.AddRecordsToLevel(interviewDataExportLevelView, sqlService);
                    }
                }
            }
        }

        public void AddOrUpdateInterviewRecords(InterviewDataExportView items, string basePath)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath,dataFile)))
            {
                foreach (var interviewDataExportLevelView in items.Levels)
                {
                    this.AddRecordsToLevel(interviewDataExportLevelView, sqlService);
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

            sqlService.ExecuteCommand(createLevelTable);

            var createLevelTableIndex = string.Format("CREATE INDEX idx{0} ON \"{1}\"({2});",
                header.LevelScopeVector.Length == 0 ? "Main" : header.LevelName, header.LevelName,
                header.LevelScopeVector.Length == 0
                    ? header.LevelIdColumnName
                    : string.Format("{0}{1}", parentId, header.LevelScopeVector.Length));
            sqlService.ExecuteCommand(createLevelTableIndex);

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
