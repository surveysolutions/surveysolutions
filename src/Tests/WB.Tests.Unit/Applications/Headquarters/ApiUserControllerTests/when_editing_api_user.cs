using System;
using Machine.Specifications;
using Moq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class when_editing_api_user : ApiUserControllerTestContext
    {
        Establish context = () =>
        {
            var userId = Guid.NewGuid();

            inputModel = new UserEditModel()
            {
                UserName = "apiTest",
                Password = "12345",
                ConfirmPassword = "12345",
                Id = userId
            };

            var user = new UserView()
            {
                PublicKey = userId,
                UserName = "apiTest1"
            };

            userViewFactory.Setup(x => x.Load(Moq.It.IsAny<UserViewInputModel>())).Returns(user);
            globalInfoProvider.Setup(x => x.IsHeadquarter).Returns(false);
            globalInfoProvider.Setup(x => x.IsSupervisor).Returns(false);
            globalInfoProvider.Setup(x => x.GetCurrentUser()).Returns(new UserLight(userId, "t"));

            controller = CreateApiUserController(commandService: commandServiceMock.Object,
                globalInfoProvider: globalInfoProvider.Object,
                userViewFactory: userViewFactory.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Edit(inputModel);
        };

        It should_return_ViewResult = () =>
            actionResult.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_execute_CreateUserCommand_onece = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.IsAny<ChangeUserCommand>(), Moq.It.IsAny<string>()), Times.Once);

        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();
        private static Mock<IUserViewFactory> userViewFactory = new Mock<IUserViewFactory>();
        private static Mock<IGlobalInfoProvider> globalInfoProvider = new Mock<IGlobalInfoProvider>();
        private static ActionResult actionResult ;
        private static ApiUserController controller;
        private static UserEditModel inputModel;
    }


}
