using System;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Ninject;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Membership;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.User;

namespace RavenQuestionnaire.Web.Tests.Membership
{
    [TestFixture]
    public class QuestionnaireMembershipProviderTests
    {
        public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public QuestionnaireMembershipProvider Provider { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandInvokerMock = new Mock<ICommandInvoker>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            IKernel kernel = new StandardKernel();
            kernel.Bind<ICommandInvoker>().ToConstant(CommandInvokerMock.Object);
            kernel.Bind<IViewRepository>().ToConstant(ViewRepositoryMock.Object);
            KernelLocator.SetKernel(kernel);
        
            Provider = new QuestionnaireMembershipProvider();
            Provider.Initialize(Provider.GetType().Name, null);
        }
      /*  [Test]
        public void WhenNewUserIsSubmittedWIthValidModel_UserIsCreate()
        {
            MembershipCreateStatus status;
            MembershipUser user = Provider.CreateUser("test", "1234", "email@test.com", null, null, true, null,
                                                      out status);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<CreateNewUserCommand>()), Times.Once());
            Assert.AreEqual(status, MembershipCreateStatus.Success);
            Assert.IsTrue(user.UserName == "test");

        }
        [Test]
        public void WhenNewUserIsSubmittedWIthInValidModel_UserRejected()
        {
            CommandInvokerMock.Setup(x => x.Execute(It.IsAny<CreateNewUserCommand>())).Throws(
                new InvalidOperationException());
            MembershipCreateStatus status;
            MembershipUser user = Provider.CreateUser("test", "1234", "invalid_email.com", null, null, true, null, out status);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<CreateNewUserCommand>()), Times.Once());
            Assert.AreEqual(status, MembershipCreateStatus.UserRejected);
            Assert.IsNull(user);
        }*/
        [Test]
        public void WhenValidateExistingUser_UserIsValid()
        {
            UserView result = new UserView("some_id", "test", "1234", "email@test.com", DateTime.Now,
                                           new[] {UserRoles.User}, false, null, null);
            ViewRepositoryMock.Setup(
                x =>
                x.Load<UserViewInputModel, UserView>(
                    It.Is<UserViewInputModel>(i => i.UserName.Equals("test") && i.Password.Equals("1234"))))
                .Returns(result);
            bool isValid = Provider.ValidateUser("test", "1234");
            Assert.IsTrue(isValid);
            ViewRepositoryMock.Verify(x => x.Load<UserViewInputModel, UserView>(
                    It.Is<UserViewInputModel>(i => i.UserName.Equals("test") && i.Password.Equals("1234"))));
        }
        [Test]
        public void WhenValidateExistingUserWithInvalidPassword_UserIsInvalid()
        {
            UserView result = new UserView("some_id", "test", "1234", "email@test.com", DateTime.Now,
                                           new[] {UserRoles.User}, false, null, null);
            ViewRepositoryMock.Setup(
                x =>
                x.Load<UserViewInputModel, UserView>(
                    It.Is<UserViewInputModel>(i => i.UserName.Equals("test") && i.Password.Equals("1234"))))
                .Returns(result);
            bool isValid = Provider.ValidateUser("test", "invalid password");
            Assert.IsTrue(!isValid);
            ViewRepositoryMock.Verify(x => x.Load<UserViewInputModel, UserView>(
                    It.Is<UserViewInputModel>(i => i.UserName.Equals("test") && i.Password.Equals("invalid password"))));
        }
        [Test]
        public void WhenValidateNotExistingUser_UserIsInvalid()
        {
            bool isValid = Provider.ValidateUser("test", "invalid password");
            Assert.IsTrue(!isValid);
            ViewRepositoryMock.Verify(x => x.Load<UserViewInputModel, UserView>(
                    It.Is<UserViewInputModel>(i => i.UserName.Equals("test") && i.Password.Equals("invalid password"))));
        }
    }
}
