using System;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class when_getting_user_packages_keys : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight { Id = userId, Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);

            syncItemsMetaContainer = CreateSyncItemsMetaContainer();
            request = CreateSyncItemsMetaContainerRequest(lastSyncedPackageId, deviceId);

            syncManagerMock = Mock.Of<ISyncManager>(x => x.GetUserPackageIdsWithOrder(userId, deviceId, lastSyncedPackageId) == syncItemsMetaContainer);

            controller = CreateSyncController(syncManager: syncManagerMock, globalInfo: globalInfo);
        };

        
        Because of = () =>
            result = controller.GetUserPackageIds(request);

        It should_return_not_null_package = () =>
            result.ShouldNotBeNull();

        It should_return_sync_package_formed_by_SyncManager = () =>
            result.ShouldEqual(syncItemsMetaContainer);

        private static SyncItemsMetaContainer result;
        private static SyncItemsMetaContainer syncItemsMetaContainer;

        private static InterviewerSyncController controller;
        private static string androidId = "Android";
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static Guid deviceId = androidId.ToGuid();
        private static string lastSyncedPackageId = "some package";
        private static SyncItemsMetaContainerRequest request;
        private static ISyncManager syncManagerMock;
    }
}