using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Ninject;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.User;
using RavenQuestionnaire.Web.Controllers;
using RavenQuestionnaire.Web.Models;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    public class AccountControllerTest
    {
        //public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public Mock<IFormsAuthentication> Authentication { get; set; }
        public AccountController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            //CommandInvokerMock = new Mock<ICommandInvoker>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            Authentication = new Mock<IFormsAuthentication>();
            IKernel kernel = new StandardKernel();
            //kernel.Bind<ICommandInvoker>().ToConstant(CommandInvokerMock.Object);
            kernel.Bind<IViewRepository>().ToConstant(ViewRepositoryMock.Object);
            KernelLocator.SetKernel(kernel);
            Controller = new AccountController(Authentication.Object);

        }
        [Test]
        public void WhenNewUserIsSubmittedWIthValidModel_UserIsCreatedAndLoggedIn()
        {
            Authentication.Setup(x => x.SignIn("test_user", false));
            Controller.Register(new RegisterModel()
            {
                UserName = "test_user",
                ConfirmPassword = "1234",
                Password = "1234",
                Email = "test@bank.com"
            });


            Authentication.Verify(x => x.SignIn("test_user", false), Times.Once());
        }
        [Test]
        public void WhenLogINWIthValidModel_UserIsLoggedIn()
        {
            // "mRqHIYxgUmCNXh1JIRItig==" is hash from "1234"
            UserView userView = new UserView(Guid.Empty, "test", "mRqHIYxgUmCNXh1JIRItig==", "test@bank.com", DateTime.Now,
                                             new[] { UserRoles.User }, false, null, Guid.Empty);
            Authentication.Setup(x => x.SignIn("test", false));
            Controller.LogOn(new LogOnModel()
            {
                Password = "1234",
                RememberMe = false,
                UserName = "test"
            }, "~/");
            Authentication.Verify(x => x.SignIn("test", false), Times.Once());
        }
    }
}
