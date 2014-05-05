using System;
using Machine.Specifications;
using Moq;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Supervisor.Controllers;

namespace WB.UI.Supervisor.Tests.AccountControllerTests
{
    [Subject(typeof(AccountController))]
    internal class AccountControllerTestsContext
    {
        protected static AccountController CreateAccountController(IFormsAuthentication auth = null, IGlobalInfoProvider globalProvider = null,
            IPasswordHasher passwordHasher = null,  IHeadquartersLoginService loginService = null,
            Func<string, string, bool> validateUserCredentials = null)
        {
            return new AccountController(
                auth ?? Mock.Of<IFormsAuthentication>(),
                globalProvider ?? Mock.Of<IGlobalInfoProvider>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                loginService ?? Mock.Of<IHeadquartersLoginService>(),
                validateUserCredentials ?? delegate { return false; });
        }
    }
}