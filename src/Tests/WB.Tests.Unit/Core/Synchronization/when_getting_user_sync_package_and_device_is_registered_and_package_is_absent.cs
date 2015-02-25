using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_getting_user_sync_package_and_device_is_registered_and_package_is_absent : SyncManagerTestContext
    {
        Establish context = () =>
        {
            tabletDocument = CreateTabletDocument(deviceId, androidId);
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>(x => x.GetById(deviceId.FormatGuid()) == tabletDocument);

            userPackageStorage = Mock.Of<IReadSideKeyValueStorage<UserSyncPackageContent>>();

            syncManager = CreateSyncManager(devices: devices, userPackageStorage: userPackageStorage);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                syncManager.ReceiveUserSyncPackage(deviceId, syncedPackageId, userId: Guid.NewGuid()));

        It should_throw_ArgumentException_exception = () =>
            exception.ShouldBeOfExactType<ArgumentException>();

        It should_throw_exception_with_message_containting__package_is_absent__ = () =>
            new[] { "package", syncedPackageId, "with user is absent" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static SyncManager syncManager;
        private static Exception exception;

        private const string androidId = "Android";
        private static Guid deviceId = androidId.ToGuid();
        private static TabletDocument tabletDocument;
        private static IReadSideRepositoryReader<TabletDocument> devices;

        private const string syncedPackageId = "some_sync_package_id";
        private static IReadSideKeyValueStorage<UserSyncPackageContent> userPackageStorage;
    }
}