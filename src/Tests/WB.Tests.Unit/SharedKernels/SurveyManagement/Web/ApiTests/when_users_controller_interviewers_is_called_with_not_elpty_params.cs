using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_users_controller_interviewers_is_called_with_not_elpty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            interviewersFactoryMock = new Mock<IUserViewFactory>();

            controller = CreateUsersController(userViewViewFactory: interviewersFactoryMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Intervievers(supervisorId, 10, 1);
        };

        It should_return_UserApiView = () =>
            actionResult.ShouldBeOfExactType<UserApiView>();

        It should_call_factory_load_once = () =>
            interviewersFactoryMock.Verify(x => x.GetInterviewers(Moq.It.IsAny<int>(), Moq.It.IsAny<int>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<bool?>(), Moq.It.IsAny<Guid?>()), Times.Once());
        
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
        private static UserApiView actionResult;
        private static UsersController controller;
        private static Mock<IUserViewFactory> interviewersFactoryMock;
    }
}
