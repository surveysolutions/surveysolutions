using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlDataExportWriter : IDataExportWriter
    {
        private const string blob = "BLOB";
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
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFileName)))
            {
                this.DeleteInterviewImpl(interviewId, sqlService);
            }


            fileSystemAccessor.DeleteDirectory(sqlDataAccessor.GetAllDataFolder(basePath));
        }

        private void DeleteInterviewImpl(Guid interviewId, ISqlService sqlService)
        {
            var tableNames = GetListofTables(sqlService);

            foreach (var tableName in tableNames)
            {
                this.DeleteFromTableByInterviewId(sqlService, tableName, interviewId.FormatGuid());
            }
        }

        public void BatchInsert(string basePath, IEnumerable<InterviewDataExportView> interviewDatas,
            IEnumerable<InterviewActionExportView> interviewActionRecords, IEnumerable<Guid> interviewsForDelete)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFileName)))
            {
                foreach (var interviewActionRecord in interviewActionRecords)
                {
                    this.InsertIntoTable(sqlService, sqlDataAccessor.InterviewActionsTableName, this.BuildInserInterviewActionParameters(interviewActionRecord));
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
            fileSystemAccessor.DeleteDirectory(sqlDataAccessor.GetAllDataFolder(basePath));

            if (interviewActionRecords.Any(action => action.Action == InterviewExportedAction.ApproveByHeadquarter))
                fileSystemAccessor.DeleteDirectory(sqlDataAccessor.GetApprovedDataFolder(basePath));
        }

        public void AddOrUpdateInterviewRecords(InterviewDataExportView items, string basePath)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFileName)))
            {
                foreach (var interviewDataExportLevelView in items.Levels)
                {
                    this.AddRecordsToLevel(interviewDataExportLevelView, sqlService);
                }
            }
        }

        public void CreateStructure(QuestionnaireExportStructure header, string basePath)
        {
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFileName)))
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
            using (var sqlService = sqlServiceFactory.CreateSqlService(fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFileName)))
            {
                this.InsertIntoTable(sqlService, sqlDataAccessor.InterviewActionsTableName, this.BuildInserInterviewActionParameters(action));
            }

            fileSystemAccessor.DeleteDirectory(sqlDataAccessor.GetAllDataFolder(basePath));

            if (action.Action == InterviewExportedAction.ApproveByHeadquarter)
                fileSystemAccessor.DeleteDirectory(sqlDataAccessor.GetApprovedDataFolder(basePath));
        }

        private void CreateHeaderForActionFile(ISqlService sqlService)
        {
            var columns = new Dictionary<string, string>();

            columns.Add(sqlDataAccessor.InterviewIdColumnName, nvarchar);
            columns.Add("Action", text);
            columns.Add("Originator", text);
            columns.Add("Role", text);
            columns.Add("Date", text);
            columns.Add("Time", text);

            CreateTable(sqlService, sqlDataAccessor.InterviewActionsTableName, columns);

        }

        private void CreateHeader(HeaderStructureForLevel header, ISqlService sqlService)
        {
            this.CreateDataTable(sqlService, header.LevelName);
        }

        private void CreateDataTable(ISqlService sqlService, string tableName)
        {
            var columns = new Dictionary<string, string> { { sqlDataAccessor.InterviewIdColumnName, nvarchar }, { sqlDataAccessor.DataColumnName, blob } };

            this.CreateTable(sqlService, tableName, columns);

            this.CreateIndexOnFieldForTable(sqlService, tableName,
                sqlDataAccessor.InterviewIdColumnName);
        }

        private void DeleteFromTableByInterviewId(ISqlService sqlService, string tableName, string interviewId)
        {
            sqlService.ExecuteCommand(
                string.Format("DELETE FROM [{0}] WHERE [{1}] = @interviewId;", tableName,
                    sqlDataAccessor.InterviewIdColumnName),
                new { interviewId });
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
            DeleteFromTableByInterviewId(sqlService, items.LevelName, items.InterviewId);

            foreach (var item in items.Records)
            {
                var parameters = this.BuildInserInterviewRecordParameters(item);

                InsertIntoTable(sqlService, items.LevelName, parameters);
            }
        }

        private List<object> BuildInserInterviewRecordParameters(InterviewDataExportRecord item)
        {
            var parameters = new List<object> { item.InterviewId.FormatGuid() };

            var parametersToConcatenate = new List<string>();

            parametersToConcatenate.Add(item.RecordId);

            foreach (var referenceValue in item.ReferenceValues)
            {
                parametersToConcatenate.Add(referenceValue);
            }

            foreach (var exportedQuestion in item.Questions)
            {
                foreach (string itemValue in exportedQuestion.Answers)
                {
                    parametersToConcatenate.Add(string.IsNullOrEmpty(itemValue)
                        ? ""
                        : itemValue);
                }
            }

            for (int i = 0; i < item.ParentRecordIds.Length; i++)
            {
                parametersToConcatenate.Add(item.ParentRecordIds[i]);
            }

            parameters.Add(this.ConcatenateDataParameters(parametersToConcatenate.ToArray()));

            return parameters;
        }

        private IEnumerable<string> GetListofTables(ISqlService sqlService)
        {
            return sqlService.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");
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

        private byte[] ConcatenateDataParameters(params string[] values)
        {
            var stringSeparator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            var concatenatedString = string.Join(stringSeparator, values.Select(v => v.Replace(stringSeparator, "")));
            byte[] bytes = new byte[concatenatedString.Length*sizeof (char)];
            Buffer.BlockCopy(concatenatedString.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
