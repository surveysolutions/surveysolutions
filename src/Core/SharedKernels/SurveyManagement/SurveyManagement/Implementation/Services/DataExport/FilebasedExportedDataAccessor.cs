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
        private readonly ILogger logger;
        private IMetadataExportService metadataExportService;

        private const string ExportedDataFolderName = "ExportedData";
        private const string ExportedFilesFolderName = "ExportedFiles";
        private readonly string pathToExportedData;
        private readonly string pathToExportedFiles;

        public FilebasedExportedDataAccessor(
            IFileSystemAccessor fileSystemAccessor,
            string folderPath,
            IMetadataExportService metadataExportService,
            ILogger logger,
            IArchiveUtils archiveUtils)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.metadataExportService = metadataExportService;
            this.logger = logger;
            this.archiveUtils = archiveUtils;
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

        public string GetArchiveFilePathForExportedTabularData(Guid questionnaireId, long version)
        {
            var archiveName = string.Format("{0}_{1}_{2}_{3}.zip",questionnaireId, version,
              ExportDataType.Tab, "All");

            return this.fileSystemAccessor.CombinePath(this.PathToExportedData, archiveName);
        }

        public string GetArchiveFilePathForExportedApprovedTabularData(Guid questionnaireId, long version)
        {
            var archiveName = string.Format("{0}_{1}_{2}_{3}.zip",questionnaireId, version,
                ExportDataType.Tab, "App");

            return this.fileSystemAccessor.CombinePath(this.PathToExportedData, archiveName);
        }
    }
}
