using System;
using System.Net;
using System.Web;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    [Ignore("no scence")]
    internal class when_pushing_file_with_invalid_credentions : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var user = new UserView();
            var userFactory = Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == user);
            
            controller = CreateSyncController();
        };

        Because of = () =>
            exception = Catch.Exception(()=> controller.PostFile(new PostFileRequest()));

        It should_exception_be_type_of_http_exception = () =>
            exception.ShouldBeOfExactType<HttpException>();

        It should_exception_error_code_be_equal_to_notacceptable = () =>
            ((HttpException)exception).GetHttpCode().ShouldEqual((int)HttpStatusCode.NotAcceptable);

        private static Exception exception;
        private static InterviewerSyncController controller;
    }
}
