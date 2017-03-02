using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.OwinSecurity;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    [Subject(typeof(UserViewFactory))]
    internal class UserViewFactoryTestContext
    {
        protected static IUserViewFactory CreateInterviewersViewFactory(IIdentityManager identityManager)
            => new UserViewFactory(identityManager);


        protected static IIdentityManager CreateQueryableReadSideRepositoryReaderWithUsers(params ApplicationUser[] users)
        {
            var mockOfApplicationUserManager = new Mock<ApplicationUserManager>();
            mockOfApplicationUserManager.Setup(x => x.Users).Returns(users.AsQueryable);
            
            return new IdentityManager(mockOfApplicationUserManager.Object,
                Mock.Of<ApplicationSignInManager>(), Mock.Of<IAuthenticationManager>());
        }

        protected static ApplicationUser CreateUser(Guid userId, Guid? supervisor, string userName,
            string deviceId = null, bool isArchived = false) => new ApplicationUser()
        {
            Id = userId,
            UserName = userName,
            Email = "",
            DeviceId = deviceId,
            IsArchived = isArchived,
            SupervisorId = supervisor
        };
    }
}