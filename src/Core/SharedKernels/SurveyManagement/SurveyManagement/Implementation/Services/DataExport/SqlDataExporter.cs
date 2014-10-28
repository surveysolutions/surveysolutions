using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlDataExporter : IDataExporter
    {
        private readonly ISqlServiceFactory sqlServiceFactory;
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly string separator;
        private readonly Func<string, string> createDataFileName;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISqlDataAccessor sqlDataAccessor;
        private const InterviewExportedAction ApproveByHeadquarterAction = InterviewExportedAction.ApproveByHeadquarter;

        public SqlDataExporter(IFileSystemAccessor fileSystemAccessor, ISqlServiceFactory sqlServiceFactory, ICsvWriterFactory csvWriterFactory, ISqlDataAccessor sqlDataAccessor)
        {
            this.sqlServiceFactory = sqlServiceFactory;
            this.csvWriterFactory = csvWriterFactory;
            this.sqlDataAccessor = sqlDataAccessor;
            this.createDataFileName = ExportFileSettings.GetContentFileName;
            this.separator = ExportFileSettings.SeparatorOfExportedDataFile;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(string basePath, string targetFolder)
        {
            var dbPath = fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFile);
            
            if (!fileSystemAccessor.IsFileExists(dbPath))
                return;
            
            using (var sqlService = sqlServiceFactory.CreateSqlService(dbPath))
            {
                var tableNames = sqlDataAccessor.GetListofTables(sqlService);

                foreach (var tableName in tableNames)
                {
                    if (tableName == sqlDataAccessor.InterviewActions)
                        continue;

                    var csvFilePath =
                        fileSystemAccessor.CombinePath(targetFolder, createDataFileName(tableName));

                    var columnNames = sqlDataAccessor.GetListOfColumns(sqlService, tableName).ToArray();
                    using (var fileStream = fileSystemAccessor.OpenOrCreateFile(csvFilePath, true))
                    using (var tabWriter = csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
                    {
                        foreach (var columnName in columnNames)
                        {
                            tabWriter.WriteField(columnName);
                        }
                        tabWriter.NextRecord();
                    }
                }
            }
        }

        public string[] GetDataFilesForQuestionnaire(string basePath)
        {
            var allDataFolderPath = sqlDataAccessor.GetAllDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(allDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(allDataFolderPath);

            fileSystemAccessor.CreateDirectory(allDataFolderPath);

            return this.ExportToTabFile(allDataFolderPath, this.fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFile), QueryAllRecodsFromTable);
        }

        public string[] GetDataFilesForQuestionnaireByInterviewsInApprovedState(string basePath)
        {
            var approvedDataFolderPath = sqlDataAccessor.GetApprovedDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(approvedDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(approvedDataFolderPath);

            fileSystemAccessor.CreateDirectory(approvedDataFolderPath);

            return this.ExportToTabFile(approvedDataFolderPath, fileSystemAccessor.CombinePath(basePath, sqlDataAccessor.DataFile),
                QueryRecordsFromTableByInterviewsInApprovedStatus);
        }

        private IEnumerable<dynamic> QueryAllRecodsFromTable(ISqlService sqlService, string tableName)
        {
            return sqlService.Query(string.Format("select * from [{0}]", tableName));
        }

        private IEnumerable<dynamic> QueryRecordsFromTableByInterviewsInApprovedStatus(ISqlService sqlService, string tableName)
        {
            var columnNames = sqlDataAccessor.GetListOfColumns(sqlService, tableName).ToArray();

            if (tableName == sqlDataAccessor.InterviewActions)
                return sqlService.Query(string.Format("select i2.* from [{0}] as i1 join [{0}] as i2 "
                    + "on i1.[Id]=i2.[Id] where i1.[Action]=@interviewAction", sqlDataAccessor.InterviewActions), new { interviewAction = ApproveByHeadquarterAction.ToString() });

            return sqlService.Query(string.Format("select [{0}].* from [{1}] join [{0}] "
                + "ON [{1}].[Id]=[{0}].[{2}] "
                + "where [{1}].[Action] = @interviewAction", tableName, sqlDataAccessor.InterviewActions,
                columnNames.Any(name => name.StartsWith(sqlDataAccessor.ParentId)) ? columnNames.Last() : "Id"), new { interviewAction = ApproveByHeadquarterAction.ToString() });
        }

        private string[] ExportToTabFile(string basePath, string dbPath, Func<ISqlService,string, IEnumerable<dynamic>> query)
        {
            var result = new List<string>();
            using (var sqlService = sqlServiceFactory.CreateSqlService(dbPath))
            {
                var tableNames = sqlDataAccessor.GetListofTables(sqlService);

                foreach (var tableName in tableNames)
                {
                    var csvFilePath =
                        fileSystemAccessor.CombinePath(basePath, createDataFileName(tableName));

                    var columnNames = sqlDataAccessor.GetListOfColumns(sqlService, tableName);

                    using (var fileStream = fileSystemAccessor.OpenOrCreateFile(csvFilePath, true))
                    using (var tabWriter = csvWriterFactory.OpenCsvWriter(fileStream, separator))
                    {
                        foreach (var columnName in columnNames)
                        {
                            tabWriter.WriteField(columnName);
                        }

                        tabWriter.NextRecord();

                        var dataSet = query(sqlService, tableName);

                        foreach (var dataRow in dataSet)
                        {
                            foreach (var cell in dataRow)
                            {
                                if(cell.Value==null)
                                    tabWriter.WriteField(string.Empty);
                                else
                                    tabWriter.WriteField(cell.Value);
                            }

                            tabWriter.NextRecord();
                        }

                        result.Add(csvFilePath);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
