using System;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class when_getting_questionnaire_sync_package : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight { Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);

            var user = new UserWebView
                       {
                           PublicKey = userId,
                           DeviceId = null
                       };
            var userFactory = Mock.Of<IUserWebViewFactory>(x => x.Load(Moq.It.IsAny<UserWebViewInputModel>()) == user);

            questionnaireSyncPackageDto = CreateQuestionnaireSyncPackageDto(packageId);

            syncManagerMock = new Mock<ISyncManager>();
            syncManagerMock.Setup(x => x.ReceiveQuestionnaireSyncPackage(deviceId, packageId, userId)).Returns(questionnaireSyncPackageDto);

            request = CreateSyncPackageRequest(packageId, deviceId);
            controller = CreateSyncController(syncManager: syncManagerMock.Object, viewFactory: userFactory, globalInfo: globalInfo);
        };

        Because of = () =>
            result = controller.GetQuestionnaireSyncPackage(request).Content.ReadAsAsync<QuestionnaireSyncPackageDto>().Result;

        It should_return_not_null_package = () =>
            result.ShouldNotBeNull();

        It should_return_sync_package_formed_by_SyncManager = () =>
            result.ShouldEqual(questionnaireSyncPackageDto);

        private static QuestionnaireSyncPackageDto result;
        private static QuestionnaireSyncPackageDto questionnaireSyncPackageDto;

        private static InterviewerSyncController controller;
        private static string androidId = "Android";
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static Guid deviceId = androidId.ToGuid();
        private static string packageId = "some package";
        private static SyncPackageRequest request;
        private static Mock<ISyncManager> syncManagerMock;
    }
}