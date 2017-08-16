using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_users_controller_interviewers_is_called_with_not_elpty_params : ApiTestContext
    {
        [OneTimeSetUp]
        public void context()
        {
            interviewersFactoryMock = new Mock<IUserViewFactory>();
            controller = CreateUsersController(userViewViewFactory: interviewersFactoryMock.Object);

            actionResult = controller.Interviewers(supervisorId, 10, 1);
        }

        [Test]
        public void should_return_UserApiView() =>
            Assert.That(actionResult, Is.InstanceOf<UserApiView>());

        [Test]
        public void should_call_factory_load_once() =>
            interviewersFactoryMock.Verify(x => x.GetInterviewers(
                Moq.It.IsAny<int>(), 
                Moq.It.IsAny<int>(), 
                Moq.It.IsAny<string>(), 
                Moq.It.IsAny<string>(),
                Moq.It.IsAny<bool>(), 
                Moq.It.IsAny<int?>(),
                Moq.It.IsAny<Guid?>(),
                Moq.It.IsAny<InterviewerFacet>()), Times.Once());
        
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
        private static UserApiView actionResult;
        private static UsersController controller;
        private static Mock<IUserViewFactory> interviewersFactoryMock;
    }
}
