﻿using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    [TestFixture]
    internal class when_users_controller_supervisors_is_called_with_not_empty_params : ApiTestContext
    {
        [OneTimeSetUp]
        public void context ()
        {
            supervisorsFactoryMock = new Mock<IUserViewFactory>();
            controller = CreateUsersController(userViewViewFactory: supervisorsFactoryMock.Object);

            actionResult = controller.Supervisors(10, 1);
        }

        
        [Test]
        public void should_return_UserApiView() =>
            Assert.That(actionResult, Is.InstanceOf<UserApiView>());

        [Test]
        public void should_call_factory_load_once() =>
            supervisorsFactoryMock.Verify(x => x.GetUsersByRole(Moq.It.IsAny<int>(), Moq.It.IsAny<int>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<UserRoles>()), Times.Once());
        
        private static UserApiView actionResult;
        private static UsersController controller;
        private static Mock<IUserViewFactory> supervisorsFactoryMock;
    }
}
