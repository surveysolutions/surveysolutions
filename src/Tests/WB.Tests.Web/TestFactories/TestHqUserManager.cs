using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Tests.Web.TestFactories
{
    public class TestHqUserManager : HqUserManager
    {
        public TestHqUserManager() : base(Mock.Of<IUserRepository>(),
            Mock.Of<IHashCompatibilityProvider>(),
            Mock.Of<IIdentityPasswordHasher>(),
            Mock.Of<IPasswordValidator>(),
            Mock.Of<IIdentityValidator>(),
            Mock.Of<ISystemLog>())
        { }
    }
}
