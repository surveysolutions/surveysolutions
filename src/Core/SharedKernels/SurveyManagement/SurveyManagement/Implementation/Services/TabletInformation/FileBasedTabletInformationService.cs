using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.TabletInformation
{
    internal class FileBasedTabletInformationService : ITabletInformationService
    {
        private readonly string basePath;
        private const string TabletInformationFolderName = "TabletInformationStorage";
        private const char Separator = '@';
        private const char USERDELIMITER = '!';
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string zipExtension = ".zip";

        public FileBasedTabletInformationService(string parentFolder, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.basePath = fileSystemAccessor.CombinePath(parentFolder, TabletInformationFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.basePath))
                fileSystemAccessor.CreateDirectory(this.basePath);
        }

        public void SaveTabletInformation(byte[] content, string androidId, UserView user)
        {
            this.fileSystemAccessor.WriteAllBytes(this.fileSystemAccessor.CombinePath(this.basePath, this.CreateFileName(androidId, user)),
                content);
        }

        public List<TabletInformationView> GetAllTabletInformationPackages()
        {
            return this.fileSystemAccessor.GetFilesInDirectory(this.basePath, $"*{this.zipExtension}")
                .Select(this.ToTabletInfoView)
                .Where(tabletInfo => tabletInfo != null)
                .OrderBy(tabletInfo => tabletInfo.CreationDate)
                .ToList();
        }

        public string GetFullPathToContentFile(string packageName)
        {
            return this.fileSystemAccessor.CombinePath(this.basePath, packageName);
        }

        private string CreateFileName(string androidId, UserView user)
        {
            string userInfo = user == null ? null : user.UserName + USERDELIMITER + user.PublicKey; 
            
            return string.Format("{0}{1}{2}{1}{3}{4}", androidId, Separator, DateTime.Now.Ticks, userInfo, this.zipExtension);
        }

        private TabletInformationView ToTabletInfoView(string filePath)
        {
            if (!filePath.EndsWith(this.zipExtension))
                return null;

            var fileName = this.fileSystemAccessor.GetFileName(filePath);
            var fileCreationTime = this.fileSystemAccessor.GetCreationTime(filePath);
            var fileSize = this.fileSystemAccessor.GetFileSize(filePath);

            var packageInfo = this.fileSystemAccessor.GetFileNameWithoutExtension(fileName).Split(Separator);

            if (packageInfo.Length < 3 || packageInfo.Length > 4)
                return null;

            var userInfo = (packageInfo.LastOrDefault() ?? "").Split(USERDELIMITER).ToList();
            var userName = userInfo.FirstOrDefault() ?? "noname";

            return new TabletInformationView
            {
                PackageName = fileName,
                AndroidId = packageInfo.FirstOrDefault() ?? "unknown device",
                CreationDate =  fileCreationTime,
                Size = fileSize,
                UserId = userInfo.LastOrDefault(),
                UserName = userName
            };
        }

        public string GetFileName(string fileName, string hostName)
        {
            var fileInfo = this.ToTabletInfoView(this.fileSystemAccessor.CombinePath(this.basePath, fileName));

            return $"{hostName}-{fileInfo.UserName.ToLower()}-{fileInfo.CreationDate.ToString("yyyyMMddTHH-mm-ss")}{this.zipExtension}";
        }
    }
}
