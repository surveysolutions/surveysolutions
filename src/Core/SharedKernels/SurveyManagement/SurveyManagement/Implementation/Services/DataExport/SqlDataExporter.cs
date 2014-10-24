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
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class SqlDataExporter : BaseSqlService, IDataExporter
    {
        private readonly IFilebaseExportRouteService filebaseExportRouteService;
        private readonly ISqlServiceFactory sqlServiceFactory;
        private readonly ICsvWriterFactory csvWriterFactory;
        private const InterviewExportedAction ApproveByHeadquarterAction = InterviewExportedAction.ApproveByHeadquarter;

        public SqlDataExporter(IFileSystemAccessor fileSystemAccessor, IFilebaseExportRouteService filebaseExportRouteService, ISqlServiceFactory sqlServiceFactory, ICsvWriterFactory csvWriterFactory)
            : base(fileSystemAccessor)
        {
            this.filebaseExportRouteService = filebaseExportRouteService;
            this.sqlServiceFactory = sqlServiceFactory;
            this.csvWriterFactory = csvWriterFactory;
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(Guid questionnaireId, long version, string targetFolder)
        {
            var dbPath = fileSystemAccessor.CombinePath(
                filebaseExportRouteService.GetFolderPathOfDataByQuestionnaire(questionnaireId, version), dataFile);
            
            if (!fileSystemAccessor.IsFileExists(dbPath))
                return;
            
            using (var sqlService = sqlServiceFactory.CreateSqlService(dbPath))
            {
                var tableNames = this.GetListofTables(sqlService);

                foreach (var tableName in tableNames)
                {
                    if (tableName == interviewActions)
                        continue;

                    var csvFilePath =
                        fileSystemAccessor.CombinePath(targetFolder, string.Format("{0}.{1}", tableName, filebaseExportRouteService.ExtensionOfExportedDataFile));

                    var columnNames = GetListOfColumns(sqlService, tableName).ToArray();
                    using (var fileStream = fileSystemAccessor.OpenOrCreateFile(csvFilePath, true))
                    using (var tabWriter = csvWriterFactory.OpenCsvWriter(fileStream, filebaseExportRouteService.SeparatorOfExportedDataFile))
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

        public string[] ExportAllDataForQuestionnaire(Guid questionnaireId, long version, Func<string, string> fileNameCreationFunc)
        {
            var basePath = filebaseExportRouteService.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);
            var allDataFolderPath = GetAllDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(allDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(allDataFolderPath);

            fileSystemAccessor.CreateDirectory(allDataFolderPath);

            return this.ExportToTabFile(allDataFolderPath, this.fileSystemAccessor.CombinePath(basePath, dataFile), QueryAllRecodsFromTable, fileNameCreationFunc);
        }

        public string[] ExportApprovedDataForQuestionnaire(Guid questionnaireId, long version, Func<string, string> fileNameCreationFunc)
        {
            var basePath = filebaseExportRouteService.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);
            var approvedDataFolderPath = GetApprovedDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(approvedDataFolderPath))
                return fileSystemAccessor.GetFilesInDirectory(approvedDataFolderPath);

            fileSystemAccessor.CreateDirectory(approvedDataFolderPath);

            return this.ExportToTabFile(approvedDataFolderPath, fileSystemAccessor.CombinePath(basePath, dataFile),
                QueryRecordsFromTableByInterviewsInApprovedStatus, fileNameCreationFunc);
        }

        private IEnumerable<dynamic> QueryAllRecodsFromTable(ISqlService sqlService, string tableName)
        {
            return sqlService.Query(string.Format("select * from [{0}]", tableName));
        }

        private IEnumerable<dynamic> QueryRecordsFromTableByInterviewsInApprovedStatus(ISqlService sqlService, string tableName)
        {
            var columnNames = GetListOfColumns(sqlService, tableName).ToArray();

            if (tableName == interviewActions)
                return sqlService.Query(string.Format("select i2.* from [{0}] as i1 join [{0}] as i2 "
                    + "on i1.[Id]=i2.[Id] where i1.[Action]=@interviewAction", interviewActions), new { interviewAction = ApproveByHeadquarterAction.ToString() });

            return sqlService.Query(string.Format("select [{0}].* from [{1}] join [{0}] "
                + "ON [{1}].[Id]=[{0}].[{2}] "
                + "where [{1}].[Action] = @interviewAction", tableName, interviewActions,
                columnNames.Any(name => name.StartsWith(parentId)) ? columnNames.Last() : "Id"), new { interviewAction = ApproveByHeadquarterAction.ToString() });
        }

        private string[] ExportToTabFile(string basePath, string dbPath, Func<ISqlService,string, IEnumerable<dynamic>> query, Func<string, string> fileNameCreationFunc)
        {
            var result = new List<string>();
            using (var sqlService = sqlServiceFactory.CreateSqlService(dbPath))
            {
                var tableNames = this.GetListofTables(sqlService);

                foreach (var tableName in tableNames)
                {
                    var csvFilePath =
                        fileSystemAccessor.CombinePath(basePath, fileNameCreationFunc(tableName));

                    var columnNames = GetListOfColumns(sqlService, tableName);

                    using (var fileStream = fileSystemAccessor.OpenOrCreateFile(csvFilePath, true))
                    using (var tabWriter = csvWriterFactory.OpenCsvWriter(fileStream, "\t "))
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
