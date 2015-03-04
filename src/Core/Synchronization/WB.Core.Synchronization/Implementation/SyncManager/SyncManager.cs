using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly ICommandService commandService;
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        private readonly IReadSideKeyValueStorage<UserSyncPackageContent> userPackageStorage;
        private readonly IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStore;

        private readonly ISyncLogger syncLogger;

        private readonly string userQueryIndexName = typeof(UserSyncPackagesByBriefFields).Name;
        private readonly string interviewGroupQueryIndexName = typeof(InterviewSyncPackagesGroupedByRoot).Name;
        private readonly string allInterviewQueryIndexName = typeof(InterviewSyncPackagesByBriefFields).Name;
        private readonly string questionnireQueryIndexName = typeof(QuestionnaireSyncPackagesByBriefFields).Name;

        public SyncManager(IReadSideRepositoryReader<TabletDocument> devices, 
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue, 
            ICommandService commandService,
            IReadSideRepositoryIndexAccessor indexAccessor,
            IReadSideKeyValueStorage<UserSyncPackageContent> userPackageStorage,
            IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore, 
            IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStore,
            ISyncLogger syncLogger)
        {
            this.devices = devices;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.commandService = commandService;
            this.indexAccessor = indexAccessor;
            this.userPackageStorage = userPackageStorage;
            this.interviewPackageContentStore = interviewPackageContentStore;
            this.questionnaireSyncPackageContentStore = questionnaireSyncPackageContentStore;
            this.syncLogger = syncLogger;
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
                this.TrackDeviceRegistration(deviceId, clientIdentifier.UserId, clientIdentifier.AppVersion, clientIdentifier.AndroidId);
            }

            this.TraceHandshake(deviceId, clientIdentifier.UserId, clientIdentifier.AppVersion);
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
                this.TrackDeviceRegistration(deviceId, interviewerId, appVersion, androidId);
            }
            this.commandService.Execute(new LinkUserToDevice(interviewerId, androidId));
            this.TrackUserLinkingRequest(androidId.ToGuid(), interviewerId, oldDeviceId);
        }

        public SyncItemsMetaContainer GetQuestionnairePackageIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var allFromLastPackageByQuestionnaire =
                this.GetUpdateFromLastPackage(userId, lastSyncedPackageId, GetGroupedQuestionnaireSyncPackage, GetLastQuestionnaireSyncPackage);

            var updateFromLastPackageByQuestionnaire = FilterDeletedQuestionnaires(allFromLastPackageByQuestionnaire);

            this.TrackArIdsRequestIfNeeded(userId, deviceId, SyncItemType.Questionnaire, lastSyncedPackageId, updateFromLastPackageByQuestionnaire);

            return new SyncItemsMetaContainer
            {
                SyncPackagesMeta = updateFromLastPackageByQuestionnaire
            };
        }

        public SyncItemsMetaContainer GetUserPackageIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var updateFromLastPakageByUser =
                this.GetUpdateFromLastPackage(userId, lastSyncedPackageId, GetGroupedUserSyncPackage, GetLastUserSyncPackage)
                    .Select(x => new SynchronizationChunkMeta(x.PackageId, x.SortIndex, x.UserId, null))
                    .ToList(); 

            this.TrackArIdsRequestIfNeeded(userId, deviceId, SyncItemType.User, lastSyncedPackageId, updateFromLastPakageByUser);

            return new SyncItemsMetaContainer
            {
                SyncPackagesMeta = updateFromLastPakageByUser.Skip(Math.Max(0, updateFromLastPakageByUser.Count() - 1)).Take(1)
            };
        }

        public SyncItemsMetaContainer GetInterviewPackageIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var allUpdatesFromLastPackage = this.GetUpdateFromLastPackage(userId, lastSyncedPackageId, GetGroupedInterviewSyncPackage, GetLastInterviewSyncPackage);

            var updateFromLastPackageByInterview =
                allUpdatesFromLastPackage.Select(
                    x => new SynchronizationChunkMeta(x.PackageId, x.SortIndex, x.UserId, x.ItemType))
                    .ToList();

            this.TrackArIdsRequestIfNeeded(userId, deviceId, SyncItemType.Interview, lastSyncedPackageId, updateFromLastPackageByInterview);

            return new SyncItemsMetaContainer
            {
                SyncPackagesMeta = updateFromLastPackageByInterview
            };
        }

        public UserSyncPackageDto ReceiveUserSyncPackage(Guid deviceId, string packageId, Guid userId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var package = this.userPackageStorage.GetById(packageId);

            if (package == null)
                throw new ArgumentException(string.Format("Package {0} with user is absent", packageId));

            this.TrackPackgeRequest(deviceId, SyncItemType.User, packageId, userId);

            return new UserSyncPackageDto
                   {
                       PackageId = package.PackageId, 
                       Content = package.Content
                   };
        }

        public QuestionnaireSyncPackageDto ReceiveQuestionnaireSyncPackage(Guid deviceId, string packageId, Guid userId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var package = this.questionnaireSyncPackageContentStore.GetById(packageId);

            if (package == null)
                throw new ArgumentException(string.Format("Package {0} with questionnaire is absent", packageId));

            this.TrackPackgeRequest(deviceId, SyncItemType.Questionnaire, packageId, userId);

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

            this.TrackPackgeRequest(deviceId, SyncItemType.Interview, packageId, userId);

            return new InterviewSyncPackageDto
                   {
                       PackageId = packageMetaInformation.PackageId,
                       Content = packageMetaInformation.Content,
                       MetaInfo = packageMetaInformation.MetaInfo
                   };
        }

        private List<SynchronizationChunkMeta> FilterDeletedQuestionnaires(IList<QuestionnaireSyncPackageMeta> packages)
        {
            var deletedQuestionnaires = packages.Where(x => x.ItemType == SyncItemType.DeleteQuestionnaire);

            return packages
                .Where(x => x.ItemType == SyncItemType.DeleteQuestionnaire || !deletedQuestionnaires.Any(p => p.QuestionnaireId == x.QuestionnaireId && p.QuestionnaireVersion == x.QuestionnaireVersion))
                .Select(x => new SynchronizationChunkMeta(x.PackageId, x.SortIndex, Guid.Empty, x.ItemType))
                .ToList();
        }

        private IQueryable<InterviewSyncPackageMeta> GetLastInterviewSyncPackage(Guid userId)
        {
           return 
                indexAccessor.Query<InterviewSyncPackageMeta>(allInterviewQueryIndexName).Where(x=>x.UserId==userId);
        }

        private IQueryable<UserSyncPackageMeta> GetLastUserSyncPackage(Guid userId)
        {
            return
              indexAccessor.Query<UserSyncPackageMeta>(userQueryIndexName).Where(x => x.UserId == userId);
        }

        private IQueryable<QuestionnaireSyncPackageMeta> GetLastQuestionnaireSyncPackage(Guid userId)
        {
            return
                indexAccessor.Query<QuestionnaireSyncPackageMeta>(questionnireQueryIndexName);
        }

        private IQueryable<InterviewSyncPackageMeta> GetGroupedInterviewSyncPackage(Guid userId, long? lastSyncedSortIndex)
        {
            var items = indexAccessor.Query<InterviewSyncPackageMeta>(interviewGroupQueryIndexName).Where(x => x.UserId == userId);
            const string deleteInterviewItemType = SyncItemType.DeleteInterview;
            var filteredItems = lastSyncedSortIndex.HasValue
                ? items.Where(x => x.SortIndex > lastSyncedSortIndex.Value)
                : items.Where(x => x.ItemType != deleteInterviewItemType);

            return filteredItems.OrderBy(x => x.SortIndex);
        }

        private IQueryable<UserSyncPackageMeta> GetGroupedUserSyncPackage(Guid userId, long? lastSyncedSortIndex)
        {
            var items = indexAccessor.Query<UserSyncPackageMeta>(userQueryIndexName).Where(x => x.UserId == userId);

            var filteredItems = lastSyncedSortIndex.HasValue
             ? items.Where(x => x.SortIndex > lastSyncedSortIndex.Value)
             : items;

            return filteredItems.OrderBy(x => x.SortIndex);
        }

        private IQueryable<QuestionnaireSyncPackageMeta> GetGroupedQuestionnaireSyncPackage(Guid userId,
            long? lastSyncedSortIndex)
        {
            var items = indexAccessor.Query<QuestionnaireSyncPackageMeta>(questionnireQueryIndexName);

            var filteredItems = lastSyncedSortIndex.HasValue
                ? items.Where(x => x.SortIndex > lastSyncedSortIndex.Value)
                : items;

            return filteredItems.OrderBy(x => x.SortIndex);
        }

        private IList<T> GetUpdateFromLastPackage<T>(Guid userId, string lastSyncedPackageId,
            Func<Guid, long?, IQueryable<T>> groupedQuery, Func<Guid, IQueryable<T>> allQuery)
            where T : IOrderableSyncPackage
        {
            if (lastSyncedPackageId == null)
            {
                return QueryAll<T>(() => groupedQuery(userId, null));
            }

            var lastSyncedPackage = allQuery(userId)
                .FirstOrDefault(x => x.PackageId == lastSyncedPackageId);

            if (lastSyncedPackage == null)
            {
                throw new SyncPackageNotFoundException(string.Format(
                    "Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            long lastSyncedSortIndex = lastSyncedPackage.SortIndex;

            return QueryAll<T>(() => groupedQuery(userId, lastSyncedSortIndex));
        }

        private void MakeSureThisDeviceIsRegisteredOrThrow(Guid deviceId)
        {
            var device = this.devices.GetById(deviceId);

            if (device == null)
            {
                throw new ArgumentException("Device was not found");
            }
        }

        private void TrackPackgeRequest(Guid deviceId, string packageType, string packageId, Guid userId)
        {
            this.syncLogger.TrackPackageRequest(deviceId, userId, packageType, packageId);
        }

        private void TrackUserLinkingRequest(Guid deviceId, Guid userId, string oldAndroidId)
        {
            if (!string.IsNullOrEmpty(oldAndroidId))
            {
                this.syncLogger.UnlinkUserFromDevice(oldAndroidId.ToGuid(), userId);
            }
            
            this.syncLogger.TrackUserLinkingRequest(deviceId, userId);
        }

        private void TraceHandshake(Guid deviceId, Guid userId, string appVersion)
        {
            this.syncLogger.TraceHandshake(deviceId, userId, appVersion);
        }

        private void TrackArIdsRequestIfNeeded(Guid userId, Guid deviceId, string packageType, string lastSyncedPackageId, IEnumerable<SynchronizationChunkMeta> updateFromLastPakage)
        {
            this.syncLogger.TrackArIdsRequest(deviceId, userId, packageType, lastSyncedPackageId, updateFromLastPakage.Select(x => x.Id).ToArray());
        }

        private void TrackDeviceRegistration(Guid deviceId, Guid userId, string appVersion, string androidId)
        {
            this.syncLogger.TrackDeviceRegistration(deviceId, userId, appVersion, androidId);
        }

       
        public IList<T> QueryAll<T>(Func<IQueryable<T>> query)
        {
            var result = new List<T>();
            int skipResults = 0;
            while (true)
            {
                var chunk = query().Skip(skipResults).ToList();

                if (!chunk.Any())
                    break;

                result.AddRange(chunk);
                skipResults = result.Count;
            }
            return result;
        }
    }
}