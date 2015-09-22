using System;
using System.Collections.Generic;
using System.Linq;

using Main.Core.Entities.SubEntities;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.Synchronization.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.TabletInformation
{
    internal class FileBasedTabletInformationService : ITabletInformationService
    {
        private readonly string basePath;
        private const string TabletInformationFolderName = "TabletInformationStorage";
        private const char Separator = '@';   
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string zipExtension = ".zip";

        private readonly IPlainStorageAccessor<TabletSyncLog> tabletDocumentsStrogeReader;
        private readonly IReadSideRepositoryReader<UserDocument> usersStorageReader;

        public FileBasedTabletInformationService(string parentFolder, 
            IFileSystemAccessor fileSystemAccessor,
            IPlainStorageAccessor<TabletSyncLog> tabletDocumentsStrogeReader, IReadSideRepositoryReader<UserDocument> usersStorageReader)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletDocumentsStrogeReader = tabletDocumentsStrogeReader;
            this.usersStorageReader = usersStorageReader;
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

        public TabletLogView GetTabletLog(string androidId)
        {
            string deviceId = androidId.ToGuid().FormatGuid();
            var tabletLogView = new TabletLogView();
            TabletSyncLog tabletLog = tabletDocumentsStrogeReader.GetById(deviceId);
            if (tabletLog == null)
                return tabletLogView;

            tabletLogView.DeviceId = Guid.Parse(deviceId);
            tabletLogView.AndroidId = tabletLog.AndroidId;
            tabletLogView.LastUpdateDate = tabletLog.LastUpdateDate;
            tabletLogView.RegistrationDate = tabletLog.RegistrationDate;

            tabletLogView.Users =
                tabletLog.RegisteredUsersOnDevice.Select(
                    x => new UserLight(x, this.GetByUserNameGetById(x))).ToList();

            foreach (var userSyncLog in tabletLog.UserSyncLog.OrderByDescending(x => x.HandshakeTime))
            {
                var userSyncLogView = new UserSyncLogView
                {
                    User =
                        new UserLight(Guid.Parse(userSyncLog.UserId),
                            this.usersStorageReader.GetById(userSyncLog.UserId).UserName)
                };

                var tabletSyncLogView = new TabletSyncLogView
                {
                    AppVersion = userSyncLog.AppVersion,
                    HandshakeTime = userSyncLog.HandshakeTime,
                    /*    PackagesTrackingInfo =
                        userSyncLog.PackagesTrackingInfo.GroupBy(x => x.PackageType)
                            .ToDictionary(x => x.Key,
                                x =>
                                    new PackagesTrackingInfo()
                                    {
                                        LastPackageId = x.Last().PackageId,
                                        PackagesRequestInfo =
                                            x.ToDictionary(y => y.PackageId, y => y.PackageSyncTime)
                                    })*/
                    PackagesTrackingInfo =
                        new[] {SyncItemType.User, SyncItemType.Interview, SyncItemType.Questionnaire, SyncItemType.QuestionnaireAssembly}.ToDictionary(
                            x => x, x => CreatePackagesTrackingInfoForPackageType(x, userSyncLog.PackagesTrackingInfo))
                };

                userSyncLogView.TabletSyncLog.Add(tabletSyncLogView);

                tabletLogView.SyncLog.Add(userSyncLogView);
            }

            return tabletLogView;
        }

        private string GetByUserNameGetById(Guid x)
        {
            var user = this.usersStorageReader.GetById(x);

            return user == null ? "UNKNOWN" : user.UserName;
        }

        private PackagesTrackingInfo CreatePackagesTrackingInfoForPackageType(string packageType,
            IList<SyncPackageTrackingInfo> packages)
        {
            var packagesFilteredByType = packages.Where(p => p.PackageType == packageType).ToArray();

            if (!packagesFilteredByType.Any())
                return new PackagesTrackingInfo() { PackagesRequestInfo = new List<SyncPackagesTrackingInfo>() };

            return new PackagesTrackingInfo()
            {
                LastPackageId = packagesFilteredByType.Last().PackageId,
                PackagesRequestInfo =
                    packagesFilteredByType.Select(x =>
                            new SyncPackagesTrackingInfo()
                            {
                                IsPackageHandledByTheClient = x.ReceivedByClient,
                                PackageId = x.PackageId,
                                PackageRequestTime = x.PackageSyncTime,
                                PackageType = x.PackageType
                            }).ToList()
            };
        }
    }
}
