﻿using System;
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
        private readonly string pathToHistoryFiles;
        

        public FilebasedExportedDataAccessor(
            IFileSystemAccessor fileSystemAccessor,
            string folderPath, 
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            IMetadataExportService metadataExportService,
            IEnvironmentContentService environmentContentService, 
            ILogger logger, 
            IArchiveUtils archiveUtils, 
            InterviewDataExportSettings interviewDataExportSettings, 
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

            if (interviewDataExportSettings.EnableInterviewHistory)
            {
                this.pathToHistoryFiles = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath,
                    interviewDataExportSettings.ExportedDataFolderName);

                if (!fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
                    fileSystemAccessor.CreateDirectory(this.pathToHistoryFiles);
            }
        }

        public string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            var result = this.GetFolderPathOfDataByQuestionnaireImpl(questionnaireId, version);
            return result;
        }

        private string GetFolderPathOfDataByQuestionnaireImpl(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData,
                string.Format("exported_data_{0}_{1}", questionnaireId, version));
        }

        public string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version)
        {
            var result = this.GetFolderPathOfFilesByQuestionnaireImpl( questionnaireId, version);

            return result;
        }

        public string GetFolderPathOfHistoryByQuestionnaire(Guid questionnaireId, long version)
        {
            return fileSystemAccessor.CombinePath(pathToHistoryFiles,
                string.Format("{0}-{1}", questionnaireId, version));
        }

        private string GetFolderPathOfFilesByQuestionnaireImpl(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles,
                string.Format("exported_files_{0}_{1}", questionnaireId, version));
        }

        public string CreateExportDataFolder(Guid questionnaireId, long version)
        {
            var folderPath = this.GetFolderPathOfDataByQuestionnaireImpl(questionnaireId, version);
            this.CreateExportFolder(folderPath);
            return folderPath;
        }

        public void CleanExportDataFolder()
        {
            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.PathToExportedData), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.PathToExportedData), (s) => this.fileSystemAccessor.DeleteFile(s));
        }

        public string CreateExportFileFolder(Guid questionnaireId, long version)
        {
            var folderPath = this.GetFolderPathOfFilesByQuestionnaireImpl(questionnaireId, version);
            this.CreateExportFolder(folderPath);
            return folderPath;
        }

        public void CleanExportFileFolder()
        {
            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.PathToExportedFiles), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.PathToExportedFiles), (s) => this.fileSystemAccessor.DeleteFile(s));
        }

        public void CleanExportHistoryFolder()
        {
            if (fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
            {
                Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.pathToHistoryFiles), (s) => this.fileSystemAccessor.DeleteDirectory(s));
                Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.pathToHistoryFiles), (s) => this.fileSystemAccessor.DeleteFile(s));
            }
        }

        public void DeleteAllDataFolder(Guid questionnaireId, long version)
        {
            var dataFolder = GetAllDataFolder(questionnaireId, version);
            fileSystemAccessor.DeleteDirectory(dataFolder);
        }

        public void DeleteApprovedDataFolder(Guid questionnaireId, long version)
        {
            var dataFolder = GetApprovedDataFolder(questionnaireId, version);
            fileSystemAccessor.DeleteDirectory(dataFolder);
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

        private void CreateExportFolder(string folderPath)
        {
            var dataFolderForTemplatePath = folderPath;

            if (this.fileSystemAccessor.IsDirectoryExists(dataFolderForTemplatePath))
            {
                string copyPath = this.PreviousCopiesFolderPath;

                this.logger.Error(string.Format("Directory for export structure already exists: {0}. Will be moved to {1}.",
                    dataFolderForTemplatePath, copyPath));

                this.fileSystemAccessor.CopyFileOrDirectory(dataFolderForTemplatePath, copyPath);

                this.logger.Info(string.Format("Existing directory for export structure {0} copied to {1}", dataFolderForTemplatePath, copyPath));

                this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);

                this.logger.Info(string.Format("Existing directory for export structure {0} deleted", dataFolderForTemplatePath));
            }

            this.fileSystemAccessor.CreateDirectory(dataFolderForTemplatePath);
        }

        public string GetFilePathToExportedCompressedData(Guid questionnaireId, long version,
            ExportDataType exportDataType)
        {
            return GetFilePathToExportedCompressedDataImpl(questionnaireId, version, exportDataType, "All",
                GetAllDataFolder, this.tabularFormatExportService.ExportInterviewsInTabularFormatAsync);
        }

        public string GetFilePathToExportedCompressedHistoryData(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = this.GetFolderPathOfHistoryByQuestionnaire(questionnaireId, version);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(pathToHistoryFiles, string.Format("exported_history_{0}_{1}.zip", questionnaireId, version));

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            var filesToArchive = new List<string>();

            if (fileSystemAccessor.IsDirectoryExists(dataDirectoryPath))
                filesToArchive.AddRange(fileSystemAccessor.GetFilesInDirectory(dataDirectoryPath));

            archiveUtils.ZipFiles(filesToArchive, archiveFilePath);

            return archiveFilePath;
        }

        public string GetFilePathToExportedApprovedCompressedData(Guid questionnaireId, long version, ExportDataType exportDataType)
        {
            return GetFilePathToExportedCompressedDataImpl(questionnaireId, version, exportDataType, "Approved",
                GetApprovedDataFolder, this.tabularFormatExportService.ExportApprovedInterviewsInTabularFormatAsync);
        }

        public string GetFilePathToExportedBinaryData(Guid questionnaireId, long version)
        {
            var fileDirectoryPath = this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, version);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.PathToExportedData, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(fileDirectoryPath)));

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

        public string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId)
        {
            return this.fileSystemAccessor.CombinePath(this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, version),
                string.Format("interview_{0}", interviewId.FormatGuid()));
        }

        protected string PathToExportedData { get { return pathToExportedData; } }

        protected string PathToExportedFiles { get { return pathToExportedFiles; } }

        protected string PreviousCopiesOfFilesFolderPath
        {
            get { return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles, string.Format("_prv_{0}", DateTime.Now.Ticks)); }
        }

        protected string PreviousCopiesFolderPath
        {
            get { return this.fileSystemAccessor.CombinePath(this.pathToExportedData, string.Format("_prv_{0}", DateTime.Now.Ticks)); }
        }

        private string GetFilePathToExportedCompressedDataImpl(
            Guid questionnaireId, 
            long version, 
            ExportDataType exportDataType, 
            string fileSuffix, 
            Func<Guid, long, string> getPathToDataFolder,
            Func<Guid, long, string, Task> exportDataToFolderAsync)
        {
            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);

            var fileName = string.Format("{0}_{1}_{2}.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath), exportDataType, fileSuffix);
            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.PathToExportedData, fileName);

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            var filesToArchive = new List<string>();

            var directoryWithExportedDataPath = getPathToDataFolder(questionnaireId, version);
            if (!fileSystemAccessor.IsDirectoryExists(directoryWithExportedDataPath))
            {
                this.fileSystemAccessor.CreateDirectory(directoryWithExportedDataPath);
                exportDataToFolderAsync(questionnaireId, version, directoryWithExportedDataPath).WaitAndUnwrapException();
            }

            var exportedTabularDataFiles = tabularFormatExportService.GetTabularDataFilesFromFolder(directoryWithExportedDataPath);
            switch (exportDataType)
            {
                case ExportDataType.Stata:
                    {
                        filesToArchive.AddRange(this.tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaire(questionnaireId, version, exportedTabularDataFiles));
                        break;
                    }
                case ExportDataType.Spss:
                    {
                        filesToArchive.AddRange(this.tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaire(questionnaireId, version, exportedTabularDataFiles));
                        break;
                    }
                case ExportDataType.Tab:
                default:
                {
                    filesToArchive.AddRange(exportedTabularDataFiles);
                    filesToArchive.AddRange(
                        this.environmentContentService.GetContentFilesForQuestionnaire(questionnaireId, version,
                            dataDirectoryPath));
                    break;
                }
            }

            archiveUtils.ZipFiles(filesToArchive, archiveFilePath);
          
            return archiveFilePath;
        }
    }
}
