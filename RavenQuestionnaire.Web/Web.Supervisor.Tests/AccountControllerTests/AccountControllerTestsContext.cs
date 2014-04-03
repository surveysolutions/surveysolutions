using System;
using Machine.Specifications;
using Moq;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.Services;
using Web.Supervisor.Controllers;

namespace Web.Supervisor.Tests.AccountControllerTests
{
    [Subject(typeof(AccountController))]
    internal class AccountControllerTestsContext
    {
        protected static AccountController CreateAccountController(IFormsAuthentication auth = null, IGlobalInfoProvider globalProvider = null,
            IPasswordHasher passwordHasher = null, IHeadquartersSynchronizer headquartersSynchronizer = null,
            Func<string, string, bool> validateUserCredentials = null)
        {
            return new AccountController(
                auth ?? Mock.Of<IFormsAuthentication>(),
                globalProvider ?? Mock.Of<IGlobalInfoProvider>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                headquartersSynchronizer ?? Mock.Of<IHeadquartersSynchronizer>(),
                validateUserCredentials ?? delegate { return false; });
        }
    }
}