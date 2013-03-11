using Moq;
using NUnit.Framework;
using Ninject;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;

using RavenQuestionnaire.Web.Controllers;
using RavenQuestionnaire.Web.Models;
using RavenQuestionnaire.Web.Tests.Stubs;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    using Microsoft.Practices.ServiceLocation;

    using NinjectAdapter;

    public class AccountControllerTest
    {
        //public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
      //  public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public Mock<IRoleProviderMock> RoleProviderMock { get; set; }
        public Mock<IFormsAuthentication> Authentication { get; set; }
        public AccountController Controller { get; set; }
        /*public Mock<RoleProvider> RoleProvider { get; set; }*/

        [SetUp]
        public void CreateObjects()
        {
            //CommandInvokerMock = new Mock<ICommandInvoker>();
            RoleProviderMock = new Mock<IRoleProviderMock>();
            Authentication = new Mock<IFormsAuthentication>();
            //RoleProvider = new Mock<RoleProvider>();
            IKernel kernel = new StandardKernel();
            //kernel.Bind<ICommandInvoker>().ToConstant(CommandInvokerMock.Object);
            kernel.Bind<IRoleProviderMock>().ToConstant(RoleProviderMock.Object);
            //kernel.Bind<RoleProvider>().ToConstant(RoleProvider.Object);
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));
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


       /*     UserView userView = new UserView(Guid.Empty, "test", "mRqHIYxgUmCNXh1JIRItig==", "test@bank.com",
                                             DateTime.Now,
                                             new[] {UserRoles.Administrator}, false, null, Guid.Empty);

            */
            RoleProviderMock.Setup(x => x.IsUserInRole("test", "Administrator"))
                .Returns(true);

          //  Authentication.Setup(x => x.SignIn("test", false));
            Controller.LogOn(new LogOnModel()
                {
                    Password = "1234",
                    UserName = "test"
                }, "~/");
            Authentication.Verify(x => x.SignIn("test", false), Times.Once());
        }
    }
}
