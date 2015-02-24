using System;
using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class when_handshaking_capi_with_supervisor_and_android_id_is_missing : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight() { Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);

            var user = new UserWebView();
            var userFactory = Mock.Of<IUserWebViewFactory>(x => x.Load(Moq.It.IsAny<UserWebViewInputModel>()) == user);
            var syncVersionProvider = Mock.Of<ISyncProtocolVersionProvider>(x => x.GetProtocolVersion() == supervisorVersion);
            var jsonUtils = Mock.Of<IJsonUtils>(x => x.Serialize(Moq.It.IsAny<RestErrorDescription>()) == "some serialized error");
            controller = CreateSyncController(viewFactory: userFactory, jsonUtils: jsonUtils, syncVersionProvider: syncVersionProvider, globalInfo: globalInfo);
        };

        Because of = () =>
            response = controller.GetHandshakePackage(new HandshakePackageRequest
                                               {
                                                   ClientId = Guid.NewGuid(), 
                                                   AndroidId = null, 
                                                   ClientRegistrationId = Guid.NewGuid(), 
                                                   Version = capiVersion
                                               });

        It should_have_NotAcceptable_status_code = () =>
            response.StatusCode.ShouldEqual(HttpStatusCode.NotAcceptable);

        private static InterviewerSyncController controller;
        private static HttpResponseMessage response;
        private static int capiVersion = 13;
        private static int supervisorVersion = 13;
    }
}