using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_users_controller_supervisors_is_called_with_not_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            supervisorsFactoryMock = new Mock<IUserViewFactory>();
            controller = CreateUsersController(userViewViewFactory: supervisorsFactoryMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Supervisors(10, 1);
        };

        It should_return_UserApiView = () =>
            actionResult.ShouldBeOfExactType<UserApiView>();

        It should_call_factory_load_once = () =>
            supervisorsFactoryMock.Verify(x => x.GetUsersByRole(Moq.It.IsAny<int>(), Moq.It.IsAny<int>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<UserRoles>()), Times.Once());
        
        private static UserApiView actionResult;
        private static UsersController controller;
        private static Mock<IUserViewFactory> supervisorsFactoryMock;
    }
}
