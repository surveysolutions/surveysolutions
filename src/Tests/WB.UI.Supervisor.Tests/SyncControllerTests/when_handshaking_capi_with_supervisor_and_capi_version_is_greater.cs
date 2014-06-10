using System;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.UI.Supervisor.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class when_handshaking_capi_with_supervisor_and_capi_version_is_greater : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var user = new UserView();
            var userFactory = Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == user);
            controller = CreateSyncController(viewFactory: userFactory, supervisorVersion: supervisorVersion);
        };

        Because of = () =>
            result = (JsonResult)controller.Handshake("some client id", Guid.NewGuid().FormatGuid(), Guid.NewGuid(), capiVersion);

        It should_return_IsErrorOccured_setted_in_true = () =>
            (result.Data as HandshakePackage). IsErrorOccured.ShouldBeTrue();

        It should_return_error_message_that_contains_words_ = () =>
            new[] { "application", "is incometible with", "supervisor", "remove your copy", "download the correct version" }.ShouldEachConformTo(
                keyword => (result.Data as HandshakePackage).ErrorMessage.ToLower().Contains(keyword));

        private static SyncController controller;
        private static JsonResult result;
        private static int capiVersion = 18;
        private static int supervisorVersion = 13;
    }
}