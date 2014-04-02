using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation
{
    internal class FileBasedTabletInformationService : ITabletInformationService
    {
        private readonly string basePath;
        private const string TabletInformationFolderName = "TabletInformationStorage";
        private const char Separator = '@';   
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string zipExtension = ".zip";

        public FileBasedTabletInformationService(string parentFolder, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.basePath = fileSystemAccessor.CombinePath(parentFolder, TabletInformationFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.basePath))
                fileSystemAccessor.CreateDirectory(this.basePath);
        }

        public void SaveTabletInformation(byte[] content, string androidId, string registrationId)
        {
            this.fileSystemAccessor.WriteAllBytes(this.fileSystemAccessor.CombinePath(this.basePath, this.CreateFileName(androidId, registrationId)),
                content);
        }

        public List<TabletInformationView> GetAllTabletInformationPackages()
        {
            var result = new List<TabletInformationView>();
            
            foreach (var filePath in this.fileSystemAccessor.GetFilesInDirectory(this.basePath))
            {
                var tabletInformationViewFormFileInfo = this.CreateTabletInformationViewFormFileInfo
                    (this.fileSystemAccessor.GetFileName(filePath), this.fileSystemAccessor.GetCreationTime(filePath),
                        this.fileSystemAccessor.GetFileSize(filePath));

                if(tabletInformationViewFormFileInfo==null)
                    continue;

                result.Add(tabletInformationViewFormFileInfo);
            }
            return result.OrderBy(r=>r.CreationDate).ToList();
        }

        public string GetFullPathToContentFile(string packageName)
        {
            return this.fileSystemAccessor.CombinePath(this.basePath, packageName);
        }

        private string CreateFileName(string androidId, string registrationId)
        {
            return string.Format("{0}{1}{2}{1}{3}{4}", androidId, Separator, registrationId, DateTime.Now.Ticks, this.zipExtension);
        }

        private TabletInformationView CreateTabletInformationViewFormFileInfo(string fileName, DateTime fileCreationTime, long fileSize)
        {
            if (!fileName.EndsWith(this.zipExtension))
                return null;

            var fileNameWithoutExtension = fileName.Replace(this.zipExtension,"");
            var separatedValues = fileNameWithoutExtension.Split(Separator);
            if (separatedValues.Length != 3)
                return null;

            return new TabletInformationView(fileName, separatedValues[0], separatedValues[1], fileCreationTime, fileSize);
        }
    }
}
