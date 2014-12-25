using System;
using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class when_handshaking_capi_with_supervisor_and_capi_version_is_greater : SyncControllerTestContext
    {
        Establish context = () =>
        {

            var userLight = new UserLight(){Name = "test"};
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);

            var user = new UserView();
            var userFactory = Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == user);
            var versionProvider = Mock.Of<ISupportedVersionProvider>(x => x.GetApplicationBuildNumber() == supervisorVersion);
            controller = CreateSyncController(viewFactory: userFactory, versionProvider: versionProvider, globalInfo: globalInfo);
        };

        Because of = () =>
            result = controller.GetHandshakePackage("some client id", Guid.NewGuid().FormatGuid(), Guid.NewGuid(), capiVersion);
        
        It should_have_NotAcceptable_status_code = () =>
            result.StatusCode.ShouldEqual(HttpStatusCode.NotAcceptable);
        
        It should_return_error_message_that_contains_specific_words = () =>
            result.Content.ReadAsStringAsync().Result.ShouldEqual("{\"Message\":\"Synchronization failed. Interviewer and Supervisor versions are different. Interviewer application has higher version '18' than Supervisor '13'. Please contact your supervisor.\"}");

        private static InterviewerSyncController controller;
        private static HttpResponseMessage result;
        private static int capiVersion = 18;
        private static int supervisorVersion = 13;
    }
}