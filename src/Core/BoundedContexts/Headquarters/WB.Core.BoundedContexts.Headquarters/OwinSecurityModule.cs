using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.Providers;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class OwinSecurityModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IUserRepository, HqUserStore>();
            registry.Bind<IHashCompatibilityProvider, HashCompatibilityProvider>();
            registry.Bind<IIdentityPasswordHasher, HqPasswordHasher>();
            registry.Bind<IPasswordHasher<HqUser>, HqPasswordHasher>();

            registry.Bind<HqUserStore, HqUserStore>();

            
            registry.Bind<IApiTokenProvider, ApiAuthTokenProvider>();
        }
    }
}
