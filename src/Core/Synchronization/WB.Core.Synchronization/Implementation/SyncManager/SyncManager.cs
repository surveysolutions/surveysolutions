using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.SyncManager
{
    internal class SyncManager : ISyncManager
    {
        private readonly IReadSideRepositoryReader<TabletDocument> devices;
        private readonly bool isSyncTrackingEnabled = true;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly ICommandService commandService;
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        private readonly IQueryableReadSideRepositoryReader<UserSyncPackage> userPackageStorage;
        private readonly IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStore;

        private readonly string userQueryIndexName = typeof(UserSyncPackagesByBriefFields).Name;
        private readonly string interviewQueryIndexName = typeof(InterviewSyncPackagesByBriefFields).Name;
        private readonly string questionnireQueryIndexName = typeof(QuestionnaireSyncPackagesByBriefFields).Name;

        public SyncManager(IReadSideRepositoryReader<TabletDocument> devices, 
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue, 
            ICommandService commandService,
            IReadSideRepositoryIndexAccessor indexAccessor, 
            IQueryableReadSideRepositoryReader<UserSyncPackage> userPackageStorage,
            IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore, 
            IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStore)
        {
            this.devices = devices;
           
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.commandService = commandService;
            this.indexAccessor = indexAccessor;
            this.userPackageStorage = userPackageStorage;
            this.interviewPackageContentStore = interviewPackageContentStore;
            this.questionnaireSyncPackageContentStore = questionnaireSyncPackageContentStore;
        }

        public HandshakePackage InitSync(ClientIdentifier clientIdentifier)
        {
            if (clientIdentifier.ClientInstanceKey == Guid.Empty)
                throw new ArgumentException("ClientInstanceKey is incorrect.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.AndroidId))
                throw new ArgumentException("AndroidId is incorrect.");

            if (string.IsNullOrWhiteSpace(clientIdentifier.AppVersion))
                throw new ArgumentException("AppVersion is incorrect.");

            Guid deviceId = clientIdentifier.AndroidId.ToGuid();
            var device = this.devices.GetById(deviceId);

            if (device == null)
            {
                this.commandService.Execute(new RegisterTabletCommand(deviceId, clientIdentifier.UserId, clientIdentifier.AppVersion, clientIdentifier.AndroidId));
            }

            this.commandService.Execute(new TrackHandshakeCommand(deviceId, clientIdentifier.UserId, clientIdentifier.AppVersion));

            return new HandshakePackage(clientIdentifier.UserId, clientIdentifier.ClientInstanceKey, Guid.NewGuid(), deviceId);
        }

        public void SendSyncItem(Guid interviewId, string item)
        {
            this.incomingSyncPackagesQueue.Enqueue(interviewId: interviewId, item: item);
        }

        public void LinkUserToDevice(Guid interviewerId, string androidId, string appVersion, string oldDeviceId)
        {
            if (interviewerId == Guid.Empty)
                throw new ArgumentException("Interview id is not set.");

            if (string.IsNullOrEmpty(androidId))
                throw new ArgumentException("Device id is not set.");

            Guid deviceId = androidId.ToGuid();
            var device = this.devices.GetById(deviceId);
            if (device == null)
            {
                this.commandService.Execute(new RegisterTabletCommand(deviceId, interviewerId, appVersion, androidId));
            }
            else
            {
                this.commandService.Execute(new LinkUserToDevice(interviewerId, androidId));
            }
            this.TrackUserLinkingRequestIfNeeded(androidId.ToGuid(), interviewerId, oldDeviceId);
        }

        public SyncItemsMetaContainer GetQuestionnaireArIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var allFromLastPakageByQuestionnaire =
                this.GetUpdateFromLastPackage(lastSyncedPackageId, this.indexAccessor.Query<QuestionnaireSyncPackageMetaInformation>(questionnireQueryIndexName));

            var updateFromLastPakageByQuestionnaire = FilterDeletedQuestionnaires(allFromLastPakageByQuestionnaire);

            this.TrackArIdsRequestIfNeeded(userId, deviceId, SyncItemType.Questionnaire, lastSyncedPackageId, updateFromLastPakageByQuestionnaire);

            return new SyncItemsMetaContainer
            {
                SyncPackagesMeta = updateFromLastPakageByQuestionnaire
            };
        }

       

        public SyncItemsMetaContainer GetUserArIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var updateFromLastPakageByUser =
                this.GetUpdateFromLastPackage(lastSyncedPackageId, this.indexAccessor.Query<UserSyncPackage>(userQueryIndexName).Where(x => x.UserId == userId))
                .Select(x => new SynchronizationChunkMeta(x.PackageId,x.SortIndex,x.UserId, null))
                .ToList(); 

            this.TrackArIdsRequestIfNeeded(userId, deviceId, SyncItemType.User, lastSyncedPackageId, updateFromLastPakageByUser);

            return new SyncItemsMetaContainer
            {
                SyncPackagesMeta = updateFromLastPakageByUser.Skip(Math.Max(0, updateFromLastPakageByUser.Count() - 1)).Take(1)
            };
        }

        public SyncItemsMetaContainer GetInterviewArIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var allUpdatesFromLastPakage = this.GetUpdateFromLastPackage(lastSyncedPackageId, this.indexAccessor.Query<InterviewSyncPackageMetaInformation>(interviewQueryIndexName).Where(x => x.UserId == userId));

            var updateFromLastPakageByInterview = FilterInterviews(allUpdatesFromLastPakage);

            this.TrackArIdsRequestIfNeeded(userId, deviceId, SyncItemType.Interview, lastSyncedPackageId, updateFromLastPakageByInterview);

            return new SyncItemsMetaContainer
            {
                SyncPackagesMeta = updateFromLastPakageByInterview
            };
        }

        public UserSyncPackageDto ReceiveUserSyncPackage(Guid deviceId, string packageId, Guid userId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var package = this.userPackageStorage.GetById(packageId);

            if (package == null)
                throw new ArgumentException(string.Format("Package {0} with user is absent", packageId));

            this.TrackPackgeRequestIfNeeded(deviceId, SyncItemType.User, packageId, userId);

            return new UserSyncPackageDto
                   {
                       PackageId = package.PackageId, 
                       Content = package.Content, 
                       UserId = package.UserId
                   };
        }

        public QuestionnaireSyncPackageDto ReceiveQuestionnaireSyncPackage(Guid deviceId, string packageId, Guid userId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var package = this.questionnaireSyncPackageContentStore.GetById(packageId);

            if (package == null)
                throw new ArgumentException(string.Format("Package {0} with questionnaire is absent", packageId));

            this.TrackPackgeRequestIfNeeded(deviceId, SyncItemType.Questionnaire, packageId, userId);

            return new QuestionnaireSyncPackageDto
                   {
                       PackageId = package.PackageId,
                       Content = package.Content,
                       MetaInfo = package.MetaInfo,
                   };
        }

        public InterviewSyncPackageDto ReceiveInterviewSyncPackage(Guid deviceId, string packageId, Guid userId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            InterviewSyncPackageContent packageMetaInformation = this.interviewPackageContentStore.GetById(packageId);
            if (packageMetaInformation == null)
                throw new ArgumentException(string.Format("Package {0} with interview is absent", packageId));

            this.TrackPackgeRequestIfNeeded(deviceId, SyncItemType.Interview, packageId, userId);

            return new InterviewSyncPackageDto
                   {
                       PackageId = packageMetaInformation.PackageId,
                       Content = packageMetaInformation.Content,
                       MetaInfo = packageMetaInformation.MetaInfo
                   };
        }

        private List<SynchronizationChunkMeta> FilterDeletedQuestionnaires(IList<QuestionnaireSyncPackageMetaInformation> packages)
        {
            var deletedQuestionnaires = packages.Where(x => x.ItemType == SyncItemType.DeleteQuestionnaire);

            return packages
                .Where(x => x.ItemType == SyncItemType.DeleteQuestionnaire || !deletedQuestionnaires.Any(p => p.QuestionnaireId == x.QuestionnaireId && p.QuestionnaireVersion == x.QuestionnaireVersion))
                .Select(x => new SynchronizationChunkMeta(x.PackageId, x.SortIndex, Guid.Empty, x.ItemType))
                .ToList();
        }

        private List<SynchronizationChunkMeta> FilterInterviews(IList<InterviewSyncPackageMetaInformation> packages)
        {
            var lastInterviewPackageMap = packages
                .GroupBy(x => x.InterviewId)
                .ToDictionary(x => x.Key, x => x.Max(y => y.SortIndex));

            return packages
                .Where(x => lastInterviewPackageMap.ContainsKey(x.InterviewId) && x.SortIndex == lastInterviewPackageMap[x.InterviewId])
                .Select(x => new SynchronizationChunkMeta(x.PackageId,x.SortIndex,x.UserId,x.ItemType))
                .ToList();
        }

        private IList<T> GetUpdateFromLastPackage<T>(string lastSyncedPackageId, IQueryable<T> items) where T : ISyncPackage
        {
            if (lastSyncedPackageId == null)
            {
                return items.OrderBy(x => x.SortIndex)
                    .QueryAll();
            }

            var lastSyncedPackage = items.FirstOrDefault(x => x.PackageId == lastSyncedPackageId);

            if (lastSyncedPackage == null)
            {
                throw new SyncPackageNotFoundException(string.Format("Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            long lastSyncedSortIndex = lastSyncedPackage.SortIndex;

            var orderedPackages = items
                .Where(x => x.SortIndex > lastSyncedSortIndex)
                .OrderBy(x => x.SortIndex)
                .QueryAll();

            return orderedPackages;
        }

        private void TrackPackgeRequestIfNeeded(Guid deviceId, string packageType, string packageId, Guid userId)
        {
            if (isSyncTrackingEnabled)
            {
                this.commandService.Execute(new TrackPackageRequestCommand(deviceId, userId, packageType, packageId));
            }
        }

        private void TrackUserLinkingRequestIfNeeded(Guid deviceId, Guid userId, string oldAndroidId)
        {
            if (isSyncTrackingEnabled)
            {
                if (!string.IsNullOrEmpty(oldAndroidId))
                {
                    Guid oldDeviceId = oldAndroidId.ToGuid();
                    var oldDevice = this.devices.GetById(oldDeviceId);
                    if (oldDevice != null)
                    {
                        this.commandService.Execute(new UnlinkUserFromDeviceCommand(oldDeviceId, userId));
                    }
                }

                var device = this.devices.GetById(deviceId);
                if (device != null)
                {
                    this.commandService.Execute(new TrackUserLinkingRequestCommand(deviceId, userId));
                }
            }
        }

        private void TrackArIdsRequestIfNeeded(Guid userId, Guid deviceId, string packageType, string lastSyncedPackageId, IEnumerable<SynchronizationChunkMeta> updateFromLastPakage)
        {
            if (isSyncTrackingEnabled)
            {
                this.commandService.Execute(
                    new TrackArIdsRequestCommand(
                        deviceId,
                        userId,
                        packageType,
                        lastSyncedPackageId,
                        updateFromLastPakage.Select(x => x.Id).ToArray()));
            }
        }

        private void MakeSureThisDeviceIsRegisteredOrThrow(Guid deviceId)
        {
            var device = this.devices.GetById(deviceId);

            if (device == null)
            {
                throw new ArgumentException("Device was not found");
            }
        }
    }
}