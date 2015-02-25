using System;
using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class when_getting_user_sync_package_and_exception_happens : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight { Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);


            syncManagerMock = new Mock<ISyncManager>();
            syncManagerMock.Setup(x => x.ReceiveUserSyncPackage(deviceId, packageId, userId)).Throws<Exception>();

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock
                .Setup(x => x.Serialize(Moq.It.IsAny<RestErrorDescription>()))
                .Returns(errorMessage)
                .Callback((RestErrorDescription error) => errorDescription = error);

            request = CreateSyncPackageRequest(packageId, deviceId);
            controller = CreateSyncController(syncManager: syncManagerMock.Object, jsonUtils: jsonUtilsMock.Object, globalInfo: globalInfo);
        };

        Because of = () =>
            result = controller.GetUserSyncPackage(request);

        It should_return_response_with_status_ServiceUnavailable = () => 
            result.StatusCode.ShouldEqual(HttpStatusCode.ServiceUnavailable);

        It should_return_response_with_ = () =>
            result.Content.ReadAsStringAsync().Result.ShouldContain(errorMessage);

        It should_return_error_message_with_code_ServerError = () =>
            errorDescription.Message.ShouldEqual(InterviewerSyncStrings.ServerError);

        private static HttpResponseMessage result;
        private static InterviewerSyncController controller;
        private static string androidId = "Android";
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static Guid deviceId = androidId.ToGuid();
        private static string packageId = "some package";
        private static SyncPackageRequest request;
        private static Mock<ISyncManager> syncManagerMock;
        private static string errorMessage = "error";
        private static RestErrorDescription errorDescription;
    }
}