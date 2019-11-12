using System;
using Microsoft.AspNet.Identity;
using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Web.TestFactories
{
    public class TestHqUserManager : HqUserManager
    {
        public TestHqUserManager() : base(Mock.Of<IUserStore<HqUser, Guid>>(),
            Mock.Of<IHashCompatibilityProvider>(),
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IIdentityValidator<string>>(),
            Mock.Of<ISystemLog>())
        { }
    }
}
