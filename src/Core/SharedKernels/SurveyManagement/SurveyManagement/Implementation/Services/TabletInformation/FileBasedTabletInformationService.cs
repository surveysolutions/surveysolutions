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

        public void SaveTabletInformation(byte[] content, string androidId, string registrationId, UserView user)
        {
            this.fileSystemAccessor.WriteAllBytes(this.fileSystemAccessor.CombinePath(this.basePath, this.CreateFileName(androidId, registrationId, user)),
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

        private string CreateFileName(string androidId, string registrationId, UserView user)
        {
            string userInfo = user == null ? null : user.UserName + USERDELIMITER + user.PublicKey; 
            
            return string.Format("{0}{1}{2}{1}{3}{1}{4}{5}", androidId, Separator, registrationId, DateTime.Now.Ticks, userInfo, this.zipExtension);
        }

        private TabletInformationView CreateTabletInformationViewFormFileInfo(string fileName, DateTime fileCreationTime, long fileSize)
        {
            if (!fileName.EndsWith(this.zipExtension))
                return null;

            var fileNameWithoutExtension = fileName.Replace(this.zipExtension,"");
            var separatedValues = fileNameWithoutExtension.Split(Separator);
            if (separatedValues.Length != 3 && separatedValues.Length != 4)
                return null;

            var tabletInfo = new TabletInformationView(fileName, separatedValues[0], separatedValues[1], fileCreationTime, fileSize);

            if (separatedValues.Length == 4 && !string.IsNullOrEmpty(separatedValues[3]))
            {
                var userInfo = separatedValues[3].Split(USERDELIMITER);
                if (userInfo.Length == 2)
                {
                    Guid userId;
                    if (Guid.TryParse(userInfo[1], out userId))
                    {
                        tabletInfo.UserId = userId;
                        tabletInfo.UserName = userInfo[0];
                    }
                }
            }

            return tabletInfo;
        }

        public string GetPackageNameWithoutRegistrationId(string packageName)
        {
            var fileNameWithoutExtension = this.fileSystemAccessor.GetFileNameWithoutExtension(packageName);
            var separatedValues = fileNameWithoutExtension.Split(Separator);
            if (separatedValues.Length != 3 && separatedValues.Length != 4)
                return packageName;

            return string.Format("{0}{1}{2}{3}", separatedValues[0], Separator, separatedValues[2], this.zipExtension);
        }

        public List<TabletInformationView> GetAllTabletInformationPackages(int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
