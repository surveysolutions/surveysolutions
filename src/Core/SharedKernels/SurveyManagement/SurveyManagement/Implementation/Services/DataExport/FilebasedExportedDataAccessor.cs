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

        protected string PathToExportedData
        {
            get { return pathToExportedData; }
        }

        protected string PathToExportedFiles
        {
            get { return pathToExportedFiles; }
        }

        public string GetArchiveFilePathForExportedTabularData(Guid questionnaireId, long version)
        {
            var archiveName = $"{questionnaireId}_{version}_Tab_All.zip";

            return this.fileSystemAccessor.CombinePath(this.PathToExportedData, archiveName);
        }

        public string GetArchiveFilePathForExportedApprovedTabularData(Guid questionnaireId, long version)
        {
            var archiveName = $"{questionnaireId}_{version}_Tab_App.zip";

            return this.fileSystemAccessor.CombinePath(this.PathToExportedData, archiveName);
        }
    }
}
