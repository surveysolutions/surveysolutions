using Core.Supervisor.Views.User;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using Web.Supervisor.API;
using Web.Supervisor.Models.API;
using It = Machine.Specifications.It;


namespace Web.Supervisor.Tests.ApiTests
{
    internal class when_users_controller_supervisors_is_called_with_not_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            supervisorsFactoryMock = new Mock<IViewFactory<UserListViewInputModel, UserListView>>();
            controller = CreateUsersController(userListViewViewFactory: supervisorsFactoryMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Supervisors(10, 1);
        };

        It should_return_UserApiView = () =>
            actionResult.ShouldBeOfExactType<UserApiView>();

        It should_call_factory_load_once = () =>
            supervisorsFactoryMock.Verify(x => x.Load(Moq.It.IsAny<UserListViewInputModel>()), Times.Once());
        
        private static UserApiView actionResult;
        private static UsersController controller;
        private static Mock<IViewFactory<UserListViewInputModel, UserListView>> supervisorsFactoryMock;
    }
}
