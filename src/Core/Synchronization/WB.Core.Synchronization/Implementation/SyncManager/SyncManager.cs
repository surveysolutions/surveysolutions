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
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Implementation.SyncManager
{
    internal class SyncManager : ISyncManager
    {
        private readonly IReadSideRepositoryReader<TabletDocument> devices;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly ICommandService commandService;

        private readonly IReadSideKeyValueStorage<UserSyncPackageContent> userPackageStorage;
        private readonly IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore;
        private readonly IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStore;
        private readonly IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader;
        private readonly IQueryableReadSideRepositoryReader<UserSyncPackageMeta> usersSyncPackagesMeta;
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireSyncPackageMeta> questionnaireSyncPackageMetaReader;

        private readonly ISyncLogger syncLogger;

        public SyncManager(IReadSideRepositoryReader<TabletDocument> devices, 
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue, 
            ICommandService commandService,
            IReadSideKeyValueStorage<UserSyncPackageContent> userPackageStorage,
            IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore, 
            IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStore,
            IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> syncPackagesMetaReader,
            IQueryableReadSideRepositoryReader<UserSyncPackageMeta> usersSyncPackagesMeta,
            IQueryableReadSideRepositoryReader<QuestionnaireSyncPackageMeta> questionnaireSyncPackageMetaReader,
            ISyncLogger syncLogger)
        {
            this.devices = devices;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.commandService = commandService;
            this.userPackageStorage = userPackageStorage;
            this.interviewPackageContentStore = interviewPackageContentStore;
            this.questionnaireSyncPackageContentStore = questionnaireSyncPackageContentStore;
            this.syncPackagesMetaReader = syncPackagesMetaReader;
            this.usersSyncPackagesMeta = usersSyncPackagesMeta;
            this.questionnaireSyncPackageMetaReader = questionnaireSyncPackageMetaReader;
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

            var updateFromLastPackageByQuestionnaire = FilterDeletedQuestionnaires(allFromLastPackageByQuestionnaire, lastSyncedPackageId);

            this.TrackArIdsRequest(userId, deviceId, SyncItemType.Questionnaire, lastSyncedPackageId, updateFromLastPackageByQuestionnaire);

            return new SyncItemsMetaContainer
            {
                SyncPackagesMeta = updateFromLastPackageByQuestionnaire
            };
        }

        public SyncItemsMetaContainer GetUserPackageIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            var updateFromLastPackageByUser =
                this.GetUpdateFromLastPackage(userId, lastSyncedPackageId, GetGroupedUserSyncPackage, GetLastUserSyncPackage)
                    .Select(x => new SynchronizationChunkMeta(x.PackageId, x.SortIndex, x.UserId, null))
                    .ToList();

            updateFromLastPackageByUser = updateFromLastPackageByUser.Skip(Math.Max(0, updateFromLastPackageByUser.Count() - 1)).Take(1).ToList();

            this.TrackArIdsRequest(userId, deviceId, SyncItemType.User, lastSyncedPackageId, updateFromLastPackageByUser);

            return new SyncItemsMetaContainer
            {
                SyncPackagesMeta = updateFromLastPackageByUser
            };
        }

        public SyncItemsMetaContainer GetInterviewPackageIdsWithOrder(Guid userId, Guid deviceId, string lastSyncedPackageId)
        {
            this.MakeSureThisDeviceIsRegisteredOrThrow(deviceId);

            IList<InterviewSyncPackageMeta> allUpdatesFromLastPackage = 
                this.GetUpdateFromLastPackage(userId, lastSyncedPackageId, GetGroupedInterviewSyncPackage, GetLastInterviewSyncPackage);

            List<SynchronizationChunkMeta> updateFromLastPackageByInterview =
                allUpdatesFromLastPackage.Select(
                    x => new SynchronizationChunkMeta(x.PackageId, x.SortIndex, x.UserId, x.ItemType))
                    .ToList();

            this.TrackArIdsRequest(userId, deviceId, SyncItemType.Interview, lastSyncedPackageId, updateFromLastPackageByInterview);

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

            this.TrackPackageRequest(deviceId, SyncItemType.User, packageId, userId);

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

            this.TrackPackageRequest(deviceId, SyncItemType.Questionnaire, packageId, userId);

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

            this.TrackPackageRequest(deviceId, SyncItemType.Interview, packageId, userId);

            return new InterviewSyncPackageDto
                   {
                       PackageId = packageMetaInformation.PackageId,
                       Content = packageMetaInformation.Content,
                       MetaInfo = packageMetaInformation.MetaInfo
                   };
        }

        private List<SynchronizationChunkMeta> FilterDeletedQuestionnaires(IList<QuestionnaireSyncPackageMeta> packages, string lastSyncedPackageId)
        {
            var deletedQuestionnaires = packages.Where(x => x.ItemType == SyncItemType.DeleteQuestionnaire);

            var result =  packages
                .Where(x => x.ItemType == SyncItemType.DeleteQuestionnaire || !deletedQuestionnaires.Any(p => p.QuestionnaireId == x.QuestionnaireId && p.QuestionnaireVersion == x.QuestionnaireVersion))
                .Select(x => new SynchronizationChunkMeta(x.PackageId, x.SortIndex, Guid.Empty, x.ItemType))
                .ToList();
            if (!string.IsNullOrEmpty(lastSyncedPackageId))
                return result;

            return result.Where(x => x.ItemType != SyncItemType.DeleteQuestionnaire).ToList();
        }

        private IQueryable<InterviewSyncPackageMeta> GetLastInterviewSyncPackage(Guid userId)
        {
            List<InterviewSyncPackageMeta> result = this.syncPackagesMetaReader.Query(_ =>
               _.Where(x => x.UserId == userId).ToList()
            );

            return result.AsQueryable();
        }

        private IQueryable<UserSyncPackageMeta> GetLastUserSyncPackage(Guid userId)
        {
            var packages = this.usersSyncPackagesMeta.Query(_ => _.Where(x => x.UserId == userId).ToList());
            return packages.AsQueryable();
        }

        private IQueryable<QuestionnaireSyncPackageMeta> GetLastQuestionnaireSyncPackage(Guid userId)
        {
            var result = this.questionnaireSyncPackageMetaReader.Query(_ => _.ToList());

            return result.AsQueryable();
        }

        private IQueryable<InterviewSyncPackageMeta> GetGroupedInterviewSyncPackage(Guid userId, long? lastSyncedSortIndex)
        {
            var packages = this.syncPackagesMetaReader.Query(_ =>
            {
                var filteredItems = _.Where(x => x.UserId == userId);

                if (lastSyncedSortIndex.HasValue)
                {
                    filteredItems = filteredItems.Where(x => x.SortIndex > lastSyncedSortIndex.Value);
                }

                filteredItems.OrderBy(x => x.SortIndex);
                return filteredItems.ToList();
            });

            IEnumerable<InterviewSyncPackageMeta> result = 
                from p in packages
                group p by p.InterviewId into g
                select g.Last();

            if (lastSyncedSortIndex == null)
            {
                result = result.Where(x => x.ItemType != SyncItemType.DeleteInterview);
            }

            return result.AsQueryable();
        }

        private IQueryable<UserSyncPackageMeta> GetGroupedUserSyncPackage(Guid userId, long? lastSyncedSortIndex)
        {
            var package = this.usersSyncPackagesMeta.Query(_ =>
            {
                var filteredItems = _.Where(x => x.UserId == userId);

                if (lastSyncedSortIndex.HasValue)
                {
                    filteredItems = filteredItems.Where(x => x.SortIndex > lastSyncedSortIndex.Value);
                }

                filteredItems = filteredItems.OrderByDescending(x => x.SortIndex);
                return filteredItems.First();
            });

            return new List<UserSyncPackageMeta>{package}.AsQueryable();
        }

        private IQueryable<QuestionnaireSyncPackageMeta> GetGroupedQuestionnaireSyncPackage(Guid userId,
            long? lastSyncedSortIndex)
        {
            var items = this.questionnaireSyncPackageMetaReader.Query(_ =>
            {
                var filteredItems = _;

                if (lastSyncedSortIndex.HasValue)
                {
                    filteredItems = filteredItems.Where(x => x.SortIndex > lastSyncedSortIndex.Value);
                }

                return filteredItems.OrderBy(x => x.SortIndex).ToList();
            });


            return items.AsQueryable();
        }

        private IList<T> GetUpdateFromLastPackage<T>(Guid userId, string lastSyncedPackageId,
            Func<Guid, long?, IQueryable<T>> groupedQuery, Func<Guid, IQueryable<T>> allQuery)
            where T : IOrderableSyncPackage
        {
            if (lastSyncedPackageId == null)
            {
                return groupedQuery(userId, null).ToList();
            }

            var queryable = allQuery(userId).ToList();
            var lastSyncedPackage = queryable
                .FirstOrDefault(x => x.PackageId == lastSyncedPackageId);

            if (lastSyncedPackage == null)
            {
                throw new SyncPackageNotFoundException(string.Format("Sync package with id {0} was not found on server", lastSyncedPackageId));
            }

            long lastSyncedSortIndex = lastSyncedPackage.SortIndex;

            return groupedQuery(userId, lastSyncedSortIndex).ToList();
        }

        private void MakeSureThisDeviceIsRegisteredOrThrow(Guid deviceId)
        {
            var device = this.devices.GetById(deviceId);

            if (device == null)
            {
                throw new ArgumentException("Device was not found");
            }
        }

        private void TrackPackageRequest(Guid deviceId, string packageType, string packageId, Guid userId)
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

        private void TrackArIdsRequest(Guid userId, Guid deviceId, string packageType, string lastSyncedPackageId, IEnumerable<SynchronizationChunkMeta> updateFromLastPakage)
        {
            this.syncLogger.TrackArIdsRequest(deviceId, userId, packageType, lastSyncedPackageId, updateFromLastPakage.Select(x => x.Id).ToArray());
        }

        private void TrackDeviceRegistration(Guid deviceId, Guid userId, string appVersion, string androidId)
        {
            this.syncLogger.TrackDeviceRegistration(deviceId, userId, appVersion, androidId);
        }
    }
}