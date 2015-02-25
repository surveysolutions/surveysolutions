using System;
using System.Collections.Generic;
using System.Linq;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_getting_user_ids_and_device_is_registered_and_last_package_id_is_not_empty_but_no_more_packages : SyncManagerTestContext
    {
        Establish context = () =>
        {
            tabletDocument = CreateTabletDocument(deviceId, androidId);
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>(x => x.GetById(deviceId.FormatGuid()) == tabletDocument);

            userSyncPackages = new List<UserSyncPackageMeta>
                               {
                                   CreateUserSyncPackage(userId, sortIndex:1),
                                   CreateUserSyncPackage(userId, sortIndex:2),
                                   CreateUserSyncPackage(userId, sortIndex:3)
                               };

            lastSyncedPackageId = userSyncPackages.Last().PackageId;

            indexAccessorMock = new Mock<IReadSideRepositoryIndexAccessor>();
            indexAccessorMock.Setup(x => x.Query<UserSyncPackageMeta>(userQueryIndexName))
                .Returns(userSyncPackages.AsQueryable());
            syncManager = CreateSyncManager(devices: devices, indexAccessor: indexAccessorMock.Object);
        };

        Because of = () =>
            result = syncManager.GetUserArIdsWithOrder(userId, deviceId, lastSyncedPackageId);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_return_empty_list = () =>
            result.SyncPackagesMeta.Count().ShouldEqual(0);

        private static SyncManager syncManager;
        private static SyncItemsMetaContainer result;

        private static string androidId = "Android";
        private static Guid deviceId = androidId.ToGuid();
        private static TabletDocument tabletDocument;
        private static IReadSideRepositoryReader<TabletDocument> devices;

        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string lastSyncedPackageId = null;

        private static Mock<IReadSideRepositoryIndexAccessor> indexAccessorMock;
        private static readonly string userQueryIndexName = typeof(UserSyncPackagesByBriefFields).Name;
        private static List<UserSyncPackageMeta> userSyncPackages;
    }
}