﻿using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadingTemplateService : IPreloadingTemplateService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IArchiveUtils archiveUtils;
        private const string FolderName = "PreLoadingTemplates";
        private readonly string path;

        public PreloadingTemplateService(IFileSystemAccessor fileSystemAccessor,string folderPath,
            ITabularFormatExportService tabularFormatExportService, IArchiveUtils archiveUtils)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabularFormatExportService = tabularFormatExportService;
            this.archiveUtils = archiveUtils;
            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);
        }

        public string GetFilePathToPreloadingTemplate(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);
            
            if (!fileSystemAccessor.IsDirectoryExists(dataDirectoryPath))
            {
                fileSystemAccessor.CreateDirectory(dataDirectoryPath);
            }

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.path, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath)));

            if (fileSystemAccessor.IsFileExists(archiveFilePath))
                return archiveFilePath;

            this.tabularFormatExportService.CreateHeaderStructureForPreloadingForQuestionnaire(new QuestionnaireIdentity(questionnaireId, version), dataDirectoryPath);

            if (fileSystemAccessor.GetFilesInDirectory(dataDirectoryPath).Length == 0)
            {
                fileSystemAccessor.DeleteDirectory(dataDirectoryPath);
                return null;
            }

            archiveUtils.ZipDirectory(dataDirectoryPath, archiveFilePath);

            return archiveFilePath;
        }

        private string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.path,
                this.fileSystemAccessor.MakeValidFileName(string.Format("template_{0}_v{1}", questionnaireId.FormatGuid(), version)));
        }
    }
}
