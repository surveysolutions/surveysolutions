using System;
using System.Data.Entity;
using System.Linq;
using Machine.Specifications;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    [Subject(typeof(UserViewFactory))]
    internal class UserViewFactoryTestContext
    {

        public class TestApplicationUserManager : ApplicationUserManager
        {
            public TestApplicationUserManager() : base(Mock.Of<IAppUserStore>())
            {
            }
        }
        protected static IUserViewFactory CreateInterviewersViewFactory(IIdentityManager identityManager)
            => new UserViewFactory(identityManager);

        protected static IIdentityManager CreateQueryableReadSideRepositoryReaderWithUsers(params ApplicationUser[] users)
        {
            var applicationUserManager = Mock.Of<TestApplicationUserManager>(x=>x.Users == users.AsQueryable());
            var authenticationManager = new Mock<IAuthenticationManager>();

            return new IdentityManager(applicationUserManager,
                new ApplicationSignInManager(applicationUserManager, authenticationManager.Object), authenticationManager.Object);
        }
    }
}