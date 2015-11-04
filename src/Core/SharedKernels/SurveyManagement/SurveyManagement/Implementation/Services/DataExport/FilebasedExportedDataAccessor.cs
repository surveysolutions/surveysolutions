using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class FilebasedExportedDataAccessor : IFilebasedExportedDataAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly ILogger logger;
        private IMetadataExportService metadataExportService;

        private const string ExportedDataFolderName = "ExportedData";
        private const string ExportedFilesFolderName = "ExportedFiles";
        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";
        private readonly string pathToExportedData;
        private readonly string pathToExportedFiles;

        public FilebasedExportedDataAccessor(
            IFileSystemAccessor fileSystemAccessor,
            string folderPath,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            IMetadataExportService metadataExportService,
            IEnvironmentContentService environmentContentService,
            ILogger logger,
            IArchiveUtils archiveUtils,
            ITabularFormatExportService tabularFormatExportService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
            this.metadataExportService = metadataExportService;
            this.environmentContentService = environmentContentService;
            this.logger = logger;
            this.archiveUtils = archiveUtils;
            this.tabularFormatExportService = tabularFormatExportService;
            this.pathToExportedData = fileSystemAccessor.CombinePath(folderPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);

            this.pathToExportedFiles = fileSystemAccessor.CombinePath(folderPath, ExportedFilesFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedFiles))
                fileSystemAccessor.CreateDirectory(this.pathToExportedFiles);
        }

        public string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            var result = this.fileSystemAccessor.CombinePath(this.pathToExportedData,
                string.Format("exported_data_{0}_{1}", questionnaireId, version));
            return result;
        }

        public string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version)
        {
            var result = this.fileSystemAccessor.CombinePath(this.pathToExportedFiles,
                string.Format("exported_files_{0}_{1}", questionnaireId, version));

            return result;
        }

        public string CreateExportFileFolder(Guid questionnaireId, long version)
        {
            var folderPath = this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, version);

            if (this.fileSystemAccessor.IsDirectoryExists(folderPath))
            {
                string copyPath = this.PreviousCopiesOfFilesFolderPath;

                this.logger.Error(
                    string.Format("Directory for export structure already exists: {0}. Will be moved to {1}.",
                        folderPath, copyPath));

                this.fileSystemAccessor.CopyFileOrDirectory(folderPath, copyPath);
                this.fileSystemAccessor.DeleteDirectory(folderPath);
            }

            this.fileSystemAccessor.CreateDirectory(folderPath);
            return folderPath;
        }

        public void CleanExportFileFolder()
        {
            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.PathToExportedFiles),
                (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.PathToExportedFiles),
                (s) => this.fileSystemAccessor.DeleteFile(s));
        }

        private string GetAllDataFolder(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(
                questionnaireId, version), allDataFolder);
        }

        private string GetApprovedDataFolder(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(
                questionnaireId, version), approvedDataFolder);
        }

        public string GetFilePathToExportedBinaryData(Guid questionnaireId, long version)
        {
            var fileDirectoryPath = this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, version);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.PathToExportedData,
                string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(fileDirectoryPath)));

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            archiveUtils.ZipDirectory(fileDirectoryPath, archiveFilePath);

            return archiveFilePath;
        }

        public string GetFilePathToExportedDDIMetadata(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);

            var fileName = string.Format("{0}_ddi.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath));
            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.PathToExportedData, fileName);

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            var filesToArchive = new List<string>
            {
                this.metadataExportService.CreateAndGetDDIMetadataFileForQuestionnaire(questionnaireId, version,
                    dataDirectoryPath)
            };

            archiveUtils.ZipFiles(filesToArchive, archiveFilePath);

            return archiveFilePath;
        }

        public string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version,
            Guid interviewId)
        {
            return
                this.fileSystemAccessor.CombinePath(this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, version),
                    string.Format("interview_{0}", interviewId.FormatGuid()));
        }

        protected string PathToExportedData
        {
            get { return pathToExportedData; }
        }

        protected string PathToExportedFiles
        {
            get { return pathToExportedFiles; }
        }

        protected string PreviousCopiesOfFilesFolderPath
        {
            get
            {
                return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles,
                    string.Format("_prv_{0}", DateTime.Now.Ticks));
            }
        }

        public void ReexportTabularDataFolder(Guid questionnaireId, long version)
        {
            var dataFolder = GetAllDataFolder(questionnaireId, version);
            if (fileSystemAccessor.IsDirectoryExists(dataFolder))
                fileSystemAccessor.DeleteDirectory(dataFolder);

            fileSystemAccessor.CreateDirectory(dataFolder);

            tabularFormatExportService.ExportInterviewsInTabularFormatAsync(questionnaireId, version, dataFolder)
                .WaitAndUnwrapException();

            CreateArchiveOfExportedTabularData(questionnaireId, version, dataFolder,
                GetArchiveFilePathForExportedTabularData(questionnaireId, version));
        }

        public string GetArchiveFilePathForExportedTabularData(Guid questionnaireId, long version)
        {
            var directoryWithExportedDataPath = GetAllDataFolder(questionnaireId, version);
            var archiveName = string.Format("{0}_{1}_{2}.zip",
                this.fileSystemAccessor.GetFileName(directoryWithExportedDataPath), ExportDataType.Tab, "App");

            return this.fileSystemAccessor.CombinePath(this.PathToExportedData, archiveName);
        }

        public string GetArchiveFilePathForExportedApprovedTabularData(Guid questionnaireId, long version)
        {
            var directoryWithExportedDataPath = this.GetApprovedDataFolder(questionnaireId, version);
            var archiveName = string.Format("{0}_{1}_{2}.zip",
                this.fileSystemAccessor.GetFileName(directoryWithExportedDataPath), ExportDataType.Tab, "App");

            return this.fileSystemAccessor.CombinePath(this.PathToExportedData, archiveName);
        }



        private void CreateArchiveOfExportedTabularData(Guid questionnaireId, long version, string directoryWithExportedData, string archiveFilePath)
        {
            if (!fileSystemAccessor.IsDirectoryExists(directoryWithExportedData))
            {
                throw new ArgumentException("folder with tabular data is absent");
            }

            var filesToArchive = new List<string>();

            var exportedTabularDataFiles =
                tabularFormatExportService.GetTabularDataFilesFromFolder(directoryWithExportedData);

            filesToArchive.AddRange(exportedTabularDataFiles);
            filesToArchive.AddRange(
                this.environmentContentService.GetContentFilesForQuestionnaire(questionnaireId, version,
                    GetFolderPathOfDataByQuestionnaire(questionnaireId, version)));

            if (fileSystemAccessor.IsFileExists(archiveFilePath))
                fileSystemAccessor.DeleteFile(archiveFilePath);
            archiveUtils.ZipFiles(filesToArchive, archiveFilePath);
        }
    }
}
