using System;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading
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
            
            if (!this.fileSystemAccessor.IsDirectoryExists(dataDirectoryPath))
            {
                this.fileSystemAccessor.CreateDirectory(dataDirectoryPath);
            }

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.path, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath)));

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                return archiveFilePath;

            this.tabularFormatExportService.CreateHeaderStructureForPreloadingForQuestionnaire(new QuestionnaireIdentity(questionnaireId, version), dataDirectoryPath);

            if (this.fileSystemAccessor.GetFilesInDirectory(dataDirectoryPath).Length == 0)
            {
                this.fileSystemAccessor.DeleteDirectory(dataDirectoryPath);
                return null;
            }

            this.archiveUtils.ZipDirectory(dataDirectoryPath, archiveFilePath);

            return archiveFilePath;
        }

        private string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.path,
                this.fileSystemAccessor.MakeValidFileName(string.Format("template_{0}_v{1}", questionnaireId.FormatGuid(), version)));
        }
    }
}
