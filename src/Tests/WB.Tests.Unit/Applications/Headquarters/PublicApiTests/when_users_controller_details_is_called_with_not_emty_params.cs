using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_users_controller_details_is_called_with_not_emty_params : ApiTestContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var userViewFactoryMock =
                Mock.Of<IUserViewFactory>(x => x.GetUser(Moq.It.IsAny<UserViewInputModel>()) == CreateUserView(userId, userName));
            
            controller = CreateUsersController(userViewViewFactory: userViewFactoryMock);

            actionResult = controller.Details(userId);
        }

        [Test]
        public void should_return_UserApiDetails() =>
            Assert.That(actionResult, Is.InstanceOf<UserApiDetails>());

        [Test]
        public void should_return_correct_user_id() =>
            Assert.That(actionResult.UserId, Is.EqualTo(userId));

        [Test]
        public void should_return_correct_user_name() =>
            Assert.That(actionResult.UserName, Is.EqualTo(userName));

        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string userName = "user";
        private static UserApiDetails actionResult;
        private static UsersController controller;
    }
}
