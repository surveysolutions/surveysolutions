using System;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Supervisor.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class when_handshaking_capi_with_supervisor_and_capi_version_is_lesser : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var user = new UserView();
            var userFactory = Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == user);
            var versionProvider = Mock.Of<ISupportedVersionProvider>(x => x.GetApplicationBuildNumber() == supervisorVersion);
            controller = CreateSyncController(viewFactory: userFactory, versionProvider: versionProvider);
        };

        Because of = () =>
            result = (JsonResult)controller.Handshake("some client id", Guid.NewGuid().FormatGuid(), Guid.NewGuid(), capiVersion);

        It should_return_IsErrorOccured_set_in_true = () =>
            (result.Data as HandshakePackage).IsErrorOccured.ShouldBeTrue();

        It should_return_error_message_that_contains_specific_words = () =>
            new[] { "must update", "before synchronizing with", "supervisor" }.ShouldEachConformTo(
                keyword => (result.Data as HandshakePackage).ErrorMessage.ToLower().Contains(keyword));

        private static SyncController controller;
        private static JsonResult result;
        private static int capiVersion = 10;
        private static int supervisorVersion = 13;
    }
}