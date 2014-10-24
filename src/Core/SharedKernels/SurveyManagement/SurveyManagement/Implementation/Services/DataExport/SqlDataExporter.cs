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
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISqlServiceFactory sqlServiceFactory;
        private readonly ICsvWriterFactory csvWriterFactory;
        private const string AllDataFolder = "AllData";
        private const string ApprovedDataFolder = "ApprovedData";

        public SqlDataExporter(IFilebaseExportRouteService filebaseExportRouteService, ISqlServiceFactory sqlServiceFactory, IFileSystemAccessor fileSystemAccessor, ICsvWriterFactory csvWriterFactory)
        {
            this.filebaseExportRouteService = filebaseExportRouteService;
            this.sqlServiceFactory = sqlServiceFactory;
            this.fileSystemAccessor = fileSystemAccessor;
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
                    using (var csv = csvWriterFactory.OpenCsvWriter(fileStream, filebaseExportRouteService.SeparatorOfExportedDataFile))
                    {
                        foreach (var columnName in columnNames)
                        {
                            csv.WriteField(columnName);
                        }
                        csv.NextRecord();
                    }
                }
            }
        }

        public string[] ExportAllDataForQuestionnaire(Guid questionnaireId, long version, Func<string, string> fileNameCreationFunc)
        {
            var basePath = filebaseExportRouteService.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);
            var allDataFolderPath = GetAllDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(allDataFolderPath))
                fileSystemAccessor.DeleteDirectory(allDataFolderPath);
               // return fileSystemAccessor.GetFilesInDirectory(allDataFolderPath);

            fileSystemAccessor.CreateDirectory(allDataFolderPath);

            return this.ExportToCSVFile(allDataFolderPath, this.fileSystemAccessor.CombinePath(basePath, dataFile), (tableName, column) => string.Format("select * from \"{0}\"", tableName), fileNameCreationFunc);
        }

        public string[] ExportApprovedDataForQuestionnaire(Guid questionnaireId, long version, Func<string, string> fileNameCreationFunc)
        {
            var basePath = filebaseExportRouteService.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);
            var approvedDataFolderPath = GetApprovedDataFolder(basePath);

            if (fileSystemAccessor.IsDirectoryExists(approvedDataFolderPath))
                fileSystemAccessor.DeleteDirectory(approvedDataFolderPath);
                //return fileSystemAccessor.GetFilesInDirectory(approvedDataFolderPath);

            fileSystemAccessor.CreateDirectory(approvedDataFolderPath);

            return ExportToCSVFile(approvedDataFolderPath, fileSystemAccessor.CombinePath(basePath, dataFile),
                CreateQueryStringForApprovedInterviewsByTableName, fileNameCreationFunc);
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

        private string[] ExportToCSVFile(string basePath, string dbPath, Func<string, IEnumerable<string>, string> createSqlQueryFormat, Func<string, string> fileNameCreationFunc)
        {
            var result = new List<string>();
            using (var sqlService = sqlServiceFactory.CreateSqlService(dbPath))
            {
                var tableNames = this.GetListofTables(sqlService);

                foreach (var tableName in tableNames)
                {
                    var csvFilePath =
                        fileSystemAccessor.CombinePath(basePath, fileNameCreationFunc(tableName));

                    var columnNames = GetListOfColumns(sqlService, tableName).ToArray();
                    using (var fileStream = fileSystemAccessor.OpenOrCreateFile(csvFilePath, true))
                    using (var csv = csvWriterFactory.OpenCsvWriter(fileStream, "\t "))
                    {
                        foreach (var columnName in columnNames)
                        {
                            csv.WriteField(columnName);
                        }

                        csv.NextRecord();
                        var dataSet = sqlService.Query(createSqlQueryFormat(tableName, columnNames));
                        foreach (var dataRow in dataSet)
                        {
                            foreach (var cell in dataRow)
                            {
                                if(cell.Value==null)
                                    csv.WriteField(string.Empty);
                                else
                                    csv.WriteField(cell.Value);
                            }

                            csv.NextRecord();
                        }

                        result.Add(csvFilePath);
                    }
                }
            }
            return result.ToArray();
        }

        private string GetAllDataFolder(string basePath)
        {
            return fileSystemAccessor.CombinePath(basePath, AllDataFolder);
        }

        private string GetApprovedDataFolder(string basePath)
        {
            return fileSystemAccessor.CombinePath(basePath, ApprovedDataFolder);
        }
    }
}
