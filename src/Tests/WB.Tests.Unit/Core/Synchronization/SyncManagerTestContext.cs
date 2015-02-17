using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.Core.Synchronization
{
    [Subject(typeof (SyncManager))]
    internal class SyncManagerTestContext
    {
        protected static SyncManager CreateSyncManager(
            IReadSideRepositoryReader<TabletDocument> devices = null,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue = null,
            ICommandService commandService = null,
            IReadSideRepositoryIndexAccessor indexAccessor = null,
            IQueryableReadSideRepositoryReader<UserSyncPackage> userPackageStorage = null,
            IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewPackageContentStore = null,
            IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnaireSyncPackageContentStore = null)
        {
            return new SyncManager(
                devices ?? Mock.Of<IReadSideRepositoryReader<TabletDocument>>(),
                incomingSyncPackagesQueue ?? Mock.Of<IIncomingSyncPackagesQueue>(),
                commandService ?? Mock.Of<ICommandService>(),
                indexAccessor ?? Mock.Of<IReadSideRepositoryIndexAccessor>(),
                userPackageStorage ?? Mock.Of<IQueryableReadSideRepositoryReader<UserSyncPackage>>(),
                interviewPackageContentStore ?? Mock.Of<IReadSideKeyValueStorage<InterviewSyncPackageContent>>(),
                questionnaireSyncPackageContentStore ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireSyncPackageContent>>());
        }

        protected static ClientIdentifier CreateClientIdentifier(Guid userId, string androidId, string appVersion)
        {
            return new ClientIdentifier
            {
                ClientInstanceKey = Guid.NewGuid(),
                AndroidId = androidId,
                AppVersion = appVersion,
                UserId = userId
            };
        }

        protected static TabletDocument CreateTabletDocument(Guid deviceId, string androidId)
        {
            return new TabletDocument
                   {
                       DeviceId = deviceId,
                       AndroidId = androidId
                   };
        }
    }
}
