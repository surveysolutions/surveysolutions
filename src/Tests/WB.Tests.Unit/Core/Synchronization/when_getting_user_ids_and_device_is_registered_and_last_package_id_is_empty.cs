using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_getting_user_ids_and_device_is_registered_and_last_package_id_is_empty : SyncManagerTestContext
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

            var writer = Stub.ReadSideRepository<UserSyncPackageMeta>();
            foreach (var package in userSyncPackages)
            {
                writer.Store(package, package.PackageId);
            }

            syncManager = CreateSyncManager(devices: devices, usersReader: writer);
        };

        Because of = () =>
            result = syncManager.GetUserPackageIdsWithOrder(userId, deviceId, lastSyncedPackageId);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_return_list_with_single_package_id = () =>
            result.SyncPackagesMeta.Count().ShouldEqual(1);

        It should_return_list_with_last_package_id = () =>
            result.SyncPackagesMeta.Select(x => x.Id).ShouldContainOnly("11111111111111111111111111111111$3");

        It should_return_list_with_ordered_by_index_items = () =>
            result.SyncPackagesMeta.Select(x => x.SortIndex).ShouldEqual(new long[] { 3 });

        private static SyncManager syncManager;
        private static SyncItemsMetaContainer result;

        private static string androidId = "Android";
        private static Guid deviceId = androidId.ToGuid();
        private static TabletDocument tabletDocument;
        private static IReadSideRepositoryReader<TabletDocument> devices;

        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string lastSyncedPackageId = null;

        private static List<UserSyncPackageMeta> userSyncPackages;
    }
}