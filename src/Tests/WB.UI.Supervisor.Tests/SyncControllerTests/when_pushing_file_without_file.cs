using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using Machine.Specifications;
using Main.Core.View;
using Main.Core.View.User;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
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
            result = controller.PostFile(interviewId).Result;

        It should_have_UnsupportedMediaType_status_code = () =>
            result.StatusCode.ShouldEqual(HttpStatusCode.NotAcceptable);

        private static HttpResponseMessage result;
        private static InterviewerSyncController controller;
        private static HttpException exception;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
    }
}
