using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class when_pushing_file_without_file : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var user = new UserView();
            var userFactory = Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == user);
            controller = CreateSyncController(viewFactory: userFactory);
        };
        Because of = () =>
            exception = Catch.Exception(() =>
                (JsonResult)controller.PostFile("login", "password", Guid.NewGuid())) as HttpException;

        It should_http_exception_be_rised = () =>
           exception.ShouldNotBeNull();

        It should_exception_http_code_be_equal_to_204 = () =>
            exception.GetHttpCode().ShouldEqual(204);

        private static SyncController controller;
        private static HttpException exception;
    }
}
