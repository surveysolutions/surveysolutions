using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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
    internal class SqlDataExportWriter : IDataExportWriter
    {
        private const string text = "TEXT";
        private const string nvarchar = "NVARCHAR(128)";
        private readonly ISqlServiceFactory sqlServiceFactory;
        private readonly ISqlDataAccessor sqlDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public SqlDataExportWriter(ISqlDataAccessor sqlDataAccessor, ISqlServiceFactory sqlServiceFactory, IFileSystemAccessor fileSystemAccessor)
        {
            this.sqlServiceFactory = sqlServiceFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.sqlDataAccessor = sqlDataAccessor;
        }

        public void DeleteInterviewRecords(string basePath, Guid interviewId)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFile)))
            {
                this.DeleteInterviewImpl(interviewId, sqlService);
            }
        }

        private void DeleteInterviewImpl(Guid interviewId, ISqlService sqlService)
        {
            var tableNames = sqlDataAccessor.GetListofTables(sqlService);
            foreach (var tableName in tableNames)
            {
                var columnNames = sqlDataAccessor.GetListOfColumns(sqlService, tableName);
                this.DeleteFromTableByInterviewId(sqlService, tableName,
                    columnNames.Any(c => c.StartsWith(sqlDataAccessor.ParentId)) ? columnNames.Last() : "Id", interviewId.FormatGuid());
            }
        }

        public void BatchInsert(string basePath, IEnumerable<InterviewDataExportView> interviewDatas,
            IEnumerable<InterviewActionExportView> interviewActionRecords, IEnumerable<Guid> interviewsForDelete)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFile)))
            {
                foreach (var interviewActionRecord in interviewActionRecords)
                {
                    this.InsertIntoTable(sqlService, sqlDataAccessor.InterviewActions, this.BuildInserInterviewActionParameters(interviewActionRecord));
                }

                foreach (var interviewIdForDelete in interviewsForDelete)
                {
                    this.DeleteInterviewImpl(interviewIdForDelete, sqlService);
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
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFile)))
            {
                foreach (var interviewDataExportLevelView in items.Levels)
                {
                    this.AddRecordsToLevel(interviewDataExportLevelView, sqlService);
                }
            }
        }

        public void CreateStructure(QuestionnaireExportStructure header, string basePath)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFile)))
            {
                foreach (var headerStructureForLevel in header.HeaderToLevelMap.Values)
                {
                    this.CreateHeader(headerStructureForLevel, sqlService);
                }
                this.CreateHeaderForActionFile(sqlService);
            }
        }

        public void AddActionRecord(InterviewActionExportView action, string basePath)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFile)))
            {
                this.InsertIntoTable(sqlService, sqlDataAccessor.InterviewActions, this.BuildInserInterviewActionParameters(action));
            }

            fileSystemAccessor.DeleteDirectory(sqlDataAccessor.GetAllDataFolder(basePath));

            if (action.Action == InterviewExportedAction.ApproveByHeadquarter)
                fileSystemAccessor.DeleteDirectory(sqlDataAccessor.GetApprovedDataFolder(basePath));
        }

        private void CreateHeaderForActionFile(ISqlService sqlService)
        {
            var columns = new Dictionary<string, string>();

            columns.Add("Id", nvarchar);
            columns.Add("Action", text);
            columns.Add("Originator", text);
            columns.Add("Role", text);
            columns.Add("Date", text);
            columns.Add("Time", text);

            CreateTable(sqlService, sqlDataAccessor.InterviewActions, columns);
        }

        private void CreateHeader(HeaderStructureForLevel header, ISqlService sqlService)
        {
            var columns = new Dictionary<string, string>();

            columns.Add(header.LevelIdColumnName, header.LevelScopeVector.Length == 0 ? nvarchar : text);

            if (header.IsTextListScope)
            {
                foreach (var name in header.ReferencedNames)
                {
                    columns.Add(name, text);
                }
            }

            foreach (ExportedHeaderItem question in header.HeaderItems.Values)
            {
                foreach (var columnName in question.ColumnNames)
                {
                    columns.Add(columnName, text);
                }
            }

            for (int i = 0; i < header.LevelScopeVector.Length; i++)
            {
                columns.Add(string.Format("{0}{1}", sqlDataAccessor.ParentId, i + 1), i == header.LevelScopeVector.Length - 1 ? nvarchar : text);
            }

            CreateTable(sqlService, header.LevelName, columns);

            CreateIndexOnFieldForTable(sqlService, header.LevelName,
                header.LevelScopeVector.Length == 0
                    ? header.LevelIdColumnName
                    : string.Format("{0}{1}", sqlDataAccessor.ParentId, header.LevelScopeVector.Length));
        }

        private void DeleteFromTableByInterviewId(ISqlService sqlService, string tableName, string idColumnName, string interviewId)
        {
            sqlService.ExecuteCommand(
                string.Format("DELETE FROM [{0}] WHERE [{1}] = @interviewId;", tableName,
                    idColumnName),
                new { interviewId });
        }

        private void InsertIntoTableWithoutVariables(ISqlService sqlService, string tableName, List<string> data)
        {

            var insertCommand = string.Format("insert into [{0}] values ({1});", tableName,
                   string.Join(",", data));

            sqlService.ExecuteCommand(insertCommand);
        }

        private void InsertIntoTable(ISqlService sqlService, string tableName, List<object> data)
        {
            var parameters = new Dictionary<string, object>();

            for (int i = 0; i < data.Count; i++)
            {
                parameters.Add("var" + (i + 1), data[i]);
            }

            var insertCommand = string.Format("insert into [{0}] values ({1});", tableName,
                   string.Join(",", parameters.Keys.Select(k => string.Format("@{0}", k))));

            sqlService.ExecuteCommand(insertCommand, parameters);
        }

        private void CreateIndexOnFieldForTable(ISqlService sqlService, string tableName, string columnName)
        {
            var createLevelTableIndex = string.Format("CREATE INDEX [idx{0}] ON [{0}]([{1}]);",
              tableName,
              columnName);
            sqlService.ExecuteCommand(createLevelTableIndex);
        }

        private void CreateTable(ISqlService sqlService, string tableName, Dictionary<string, string> columns)
        {
            var createLevelTable = string.Format("create table [{0}] ({1});", tableName,
                string.Join(",", columns.Select(c => string.Format("[{0}] {1}", c.Key, c.Value))));

            sqlService.ExecuteCommand(createLevelTable);
        }

        private void AddRecordsToLevel(InterviewDataExportLevelView items, ISqlService sqlService)
        {
            var isMainLevel = items.LevelVector.Length == 0;

            DeleteFromTableByInterviewId(sqlService, items.LevelName,
                isMainLevel ? "Id" : string.Format("{0}{1}", sqlDataAccessor.ParentId, items.LevelVector.Length), items.InterviewId);

            foreach (var item in items.Records)
            {
                var parameters = this.BuildInserInterviewRecordParameters(item);

                InsertIntoTableWithoutVariables(sqlService, items.LevelName, parameters);
            }
        }

        private List<string> BuildInserInterviewRecordParameters(InterviewDataExportRecord item)
        {
            var parameters = new List<string> { this.QuoteString(item.RecordId) };

            foreach (var referenceValue in item.ReferenceValues)
            {
                parameters.Add(this.QuoteString(referenceValue) );
            }

            foreach (var exportedQuestion in item.Questions)
            {
                foreach (string itemValue in exportedQuestion.Answers)
                {
                    parameters.Add(string.IsNullOrEmpty(itemValue)
                        ? "null"
                        : this.QuoteString(itemValue));
                }
            }

            for (int i = 0; i < item.ParentRecordIds.Length; i++)
            {
                parameters.Add(this.QuoteString(item.ParentRecordIds[i]));
            }
            return parameters;
        }

        private List<object> BuildInserInterviewActionParameters(InterviewActionExportView action)
        {
            var parameters = new List<object>();
            parameters.Add(action.InterviewId);
            parameters.Add(action.Action.ToString());
            parameters.Add(action.Originator);
            parameters.Add(action.Role);
            parameters.Add(action.Timestamp.ToString("d", CultureInfo.InvariantCulture));
            parameters.Add(action.Timestamp.ToString("T", CultureInfo.InvariantCulture));
            return parameters;
        }

        private string QuoteString(string val)
        {
            return "'" + val.Replace("'", "''")+"'";
        }
    }
}
