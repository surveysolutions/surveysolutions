using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Synchronization
{
    internal class when_mark_sync_package_as_successfully_handled : SyncManagerTestContext
    {
        Establish context = () =>
        {
            syncManager = CreateSyncManager(syncLogger: syncLogger.Object);
        };

        Because of = () => syncManager.MarkPackageAsSuccessfullyHandled(syncedPackageId, deviceId, userId: Guid.NewGuid());

        It should_mark_in_sync_log_package_as_successful = () =>
            syncLogger.Verify(x => x.MarkPackageAsSuccessfullyHandled(deviceId, Moq.It.IsAny<Guid>(), syncedPackageId));

        private static SyncManager syncManager;

        private static Guid deviceId = "Android".ToGuid();

        private const string syncedPackageId = "some_sync_package_id";
        private static Mock<ISyncLogger> syncLogger =new Mock<ISyncLogger>();
    }
}