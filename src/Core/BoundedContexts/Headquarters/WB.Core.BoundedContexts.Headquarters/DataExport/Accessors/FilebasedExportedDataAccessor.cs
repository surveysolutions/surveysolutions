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
        private readonly string pathToExportedData;

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
        }

        public string GetFilePathToExportedDDIMetadata(QuestionnaireIdentity questionnaireId)
        {
            var archiveFilePath = GetArchiveFilePathForDDIMetadata(questionnaireId);

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                return archiveFilePath;

            var filesToArchive = new List<string>
            {
                this.metadataExportService.CreateAndGetDDIMetadataFileForQuestionnaire(questionnaireId.QuestionnaireId, questionnaireId.Version,
                    pathToExportedData)
            };

            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath);

            return archiveFilePath;
        }

        public string GetArchiveFilePathForExportedData(QuestionnaireIdentity questionnaireId, DataExportFormat format)
        {
            var archiveName = $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_{format}_All.zip";

            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, archiveName);
        }

        public string GetArchiveFilePathForExportedApprovedData(QuestionnaireIdentity questionnaireId, DataExportFormat format)
        {
            var archiveName = $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_{format}_App.zip";
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, archiveName);
        }

        protected string GetArchiveFilePathForDDIMetadata(QuestionnaireIdentity questionnaireId)
        {
            var archiveName = $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_ddi.zip";
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, archiveName);
        }
    }
}
