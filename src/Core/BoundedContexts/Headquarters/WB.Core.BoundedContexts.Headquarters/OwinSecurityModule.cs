using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
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
            registry.Bind<IPasswordValidator, HqPasswordValidator>();
            registry.Bind<IIdentityValidator, HqUserValidator>();

            registry.Bind<IUserRepository, HqUserStore>();

            
            registry.Bind<HqUserManager>();

            registry.Bind<IApiTokenProvider, ApiAuthTokenProvider>();
            registry.Bind<IAuthorizedUser, AuthorizedUser>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
            => Task.CompletedTask;
    }
}
