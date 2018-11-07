using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.InterviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    [TestOf(typeof(UserViewFactory))]
    internal class UserViewFactoryTestContext
    {
        protected static UserViewFactory CreateInterviewersViewFactory(IUserRepository userRepository)
            => new UserViewFactory(userRepository, Mock.Of<IInterviewerProfileFactory>());

        protected static IUserRepository CreateQueryableReadSideRepositoryReaderWithUsers(params HqUser[] users)
            => Mock.Of<IUserRepository>(x => x.Users == users.AsQueryable());
    }
}
