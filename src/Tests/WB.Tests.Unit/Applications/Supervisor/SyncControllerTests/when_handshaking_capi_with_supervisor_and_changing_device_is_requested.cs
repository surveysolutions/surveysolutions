using System;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class when_handshaking_capi_with_supervisor_and_changing_device_is_requested : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight() { Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);

            var user = new UserWebView
                       {
                           PublicKey = userId,
                           DeviceId = oldAndroidId
                       };
            var userFactory = Mock.Of<IUserWebViewFactory>(x => x.Load(Moq.It.IsAny<UserWebViewInputModel>()) == user);
            var syncVersionProvider = Mock.Of<ISyncProtocolVersionProvider>(x => x.GetProtocolVersion() == supervisorVersion);

            handshakePackage = Mock.Of<HandshakePackage>();

            syncManagerMock = new Mock<ISyncManager>();

            syncManagerMock.Setup(x => x.InitSync(Moq.It.IsAny<ClientIdentifier>())).Returns(handshakePackage);

            controller = CreateSyncController(syncManager: syncManagerMock.Object, viewFactory: userFactory, syncVersionProvider: syncVersionProvider, globalInfo: globalInfo);
        };

        Because of = () =>
            result = controller.GetHandshakePackage(new HandshakePackageRequest
                                                    {
                                                        AndroidId = androidId,
                                                        ClientId = Guid.NewGuid(),
                                                        ClientRegistrationId = Guid.NewGuid(),
                                                        Version = capiVersion,
                                                        ShouldDeviceBeLinkedToUser = true
                                                    });

        It should_call_LinkUserToDevice_method = () =>
            syncManagerMock.Verify(
                x => x.LinkUserToDevice(userId, androidId, "13", oldAndroidId),
                Times.Once);
     
        It should_call_InitSync_method = () => 
            syncManagerMock.Verify(
                x => x.InitSync(Moq.It.Is<ClientIdentifier>(i => i.AndroidId == androidId && i.AppVersion == "13")), 
                Times.Once);

        It should_return_handshake_package_formed_by_SyncManager = () => 
            result.ShouldEqual(handshakePackage);

        private static HandshakePackage result;
        private static HandshakePackage handshakePackage;
        private static InterviewerSyncController controller;
        private static int capiVersion = 13;
        private static int supervisorVersion = 13;
        private static string androidId = "Android";
        private static string oldAndroidId = "Old Android";
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");

        private static Mock<ISyncManager> syncManagerMock;
    }
}