using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
    internal class when_getting_questionnaire_sync_package_and_exception_happens : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var userLight = new UserLight { Name = "test" };
            var globalInfo = Mock.Of<IGlobalInfoProvider>(x => x.GetCurrentUser() == userLight);


            syncManagerMock = new Mock<ISyncManager>();
            syncManagerMock.Setup(x => x.ReceiveQuestionnaireSyncPackage(deviceId, packageId, userId)).Throws<Exception>();

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock
                .Setup(x => x.Serialize(Moq.It.IsAny<RestErrorDescription>()))
                .Returns(errorMessage)
                .Callback((RestErrorDescription error) => errorDescription = error);

            request = CreateSyncPackageRequest(packageId, deviceId);
            controller = CreateSyncController(syncManager: syncManagerMock.Object, jsonUtils: jsonUtilsMock.Object, globalInfo: globalInfo);
        };

        Because of = () =>
            exception = Catch.Exception(() => controller.GetQuestionnaireSyncPackage(request));

        It should_throw_HttpResponse_exception = () =>
            exception.ShouldBeOfExactType<HttpResponseException>();

        It should_return_response_with_status_ServiceUnavailable = () =>
           ((HttpResponseException)exception).Response.StatusCode.ShouldEqual(HttpStatusCode.ServiceUnavailable);

        It should_return_response_with_error_message_specified = () =>
            ((HttpResponseException)exception).Response.ReasonPhrase.ShouldContain(errorMessage);

        It should_return_error_message_with_code_ServerError = () =>
            errorDescription.Message.ShouldEqual(InterviewerSyncStrings.ServerError);

        private static Exception exception;
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