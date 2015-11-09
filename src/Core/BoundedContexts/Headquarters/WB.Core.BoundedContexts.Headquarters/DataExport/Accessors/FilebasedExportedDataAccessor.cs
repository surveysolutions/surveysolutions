using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
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
            InterviewDataExportSettings interviewDataExportSettings,
            IMetadataExportService metadataExportService,
            ILogger logger,
            IArchiveUtils archiveUtils)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.metadataExportService = metadataExportService;
            this.logger = logger;
            this.archiveUtils = archiveUtils;
            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);

            this.pathToExportedFiles = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, ExportedFilesFolderName);

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

            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath);

            return archiveFilePath;
        }

        protected string PathToExportedData
        {
            get { return this.pathToExportedData; }
        }

        protected string PathToExportedFiles
        {
            get { return this.pathToExportedFiles; }
        }

        public string GetArchiveFilePathForExportedData(QuestionnaireIdentity questionnaireId, DataExportFormat format)
        {
            var archiveName = $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_{format}_All.zip";

            return this.fileSystemAccessor.CombinePath(this.PathToExportedData, archiveName);
        }

        public string GetArchiveFilePathForExportedApprovedData(QuestionnaireIdentity questionnaireId, DataExportFormat format)
        {
            var archiveName = $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_{format}_App.zip";
            return this.fileSystemAccessor.CombinePath(this.PathToExportedData, archiveName);
        }
    }
}
