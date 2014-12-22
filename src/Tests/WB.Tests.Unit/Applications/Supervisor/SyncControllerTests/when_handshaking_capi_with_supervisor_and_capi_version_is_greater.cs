using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
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
            exception = Catch.Exception(() => controller.GetHandshakePackage(new HandshakePackageRequest() { ClientId = Guid.NewGuid(), AndroidId = Guid.NewGuid().FormatGuid(), ClientRegistrationId = Guid.NewGuid(), Version = capiVersion }));

        It should_exception_be_type_of_HttpResponseException = () =>
            exception.ShouldBeOfExactType<HttpResponseException>();

        It should_have_NotAcceptable_status_code = () =>
            ((HttpResponseException)exception).Response.StatusCode.ShouldEqual(HttpStatusCode.NotAcceptable);

        private static InterviewerSyncController controller;
        private static Exception exception;
        private static int capiVersion = 18;
        private static int supervisorVersion = 13;
    }
}