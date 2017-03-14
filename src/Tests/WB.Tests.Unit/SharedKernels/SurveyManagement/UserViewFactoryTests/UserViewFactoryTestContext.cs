using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    [Subject(typeof(UserViewFactory))]
    internal class UserViewFactoryTestContext
    {

        public class TestApplicationUserManager : HqUserManager
        {
            public TestApplicationUserManager() : base(Mock.Of<IUserStore<HqUser, Guid>>())
            {
            }
        }
        protected static IUserViewFactory CreateInterviewersViewFactory(IUserRepository userRepository)
            => new UserViewFactory(userRepository);

        protected static IUserRepository CreateQueryableReadSideRepositoryReaderWithUsers(params HqUser[] users)
            => Mock.Of<IUserRepository>(x => x.Users == users.AsQueryable());
    }
}