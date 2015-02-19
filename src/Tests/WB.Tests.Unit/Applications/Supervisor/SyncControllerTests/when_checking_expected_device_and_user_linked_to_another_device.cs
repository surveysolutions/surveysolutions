using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class when_checking_expected_device_and_user_linked_to_another_device : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight() { Id = userId,  Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);

            var user = new UserWebView
                       {
                           PublicKey = userId,
                           DeviceId = oldAndroidId
                       };
            var userFactory = Mock.Of<IUserWebViewFactory>(x => x.Load(Moq.It.IsAny<UserWebViewInputModel>()) == user);

            controller = CreateSyncController(viewFactory: userFactory, globalInfo: globalInfo);
        };

        Because of = () =>
            result = controller.CheckExpectedDevice(androidId);

        It should_return_false_as_result = () => 
            result.ShouldBeFalse();

        private static bool result;
        private static InterviewerSyncController controller;
        private static string androidId = "Android";
        private static string oldAndroidId = "Old Android";
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
    }
}