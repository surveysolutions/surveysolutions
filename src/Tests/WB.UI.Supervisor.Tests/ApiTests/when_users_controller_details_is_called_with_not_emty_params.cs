﻿using System;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.UI.Supervisor.API;
using WB.UI.Supervisor.Models.API;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.ApiTests
{
    internal class when_users_controller_details_is_called_with_not_emty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            var userViewFactoryMock =
                Mock.Of<IViewFactory<UserViewInputModel, UserView>>(x => x.Load(Moq.It.IsAny<UserViewInputModel>()) == CreateUserView(userId, userName)); ;
            
            controller = CreateUsersController(userViewViewFactory: userViewFactoryMock);
        };

        Because of = () =>
        {
            actionResult = controller.Details(userId);
        };

        It should_return_UserApiDetails = () =>
            actionResult.ShouldBeOfExactType<UserApiDetails>();

        It should_return_correct_user_id = () =>
            actionResult.UserId.ShouldEqual(userId);

        It should_return_correct_user_name = () =>
            actionResult.UserName.ShouldEqual(userName);

        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string userName = "user";
        private static UserApiDetails actionResult;
        private static UsersController controller;
    }
}
