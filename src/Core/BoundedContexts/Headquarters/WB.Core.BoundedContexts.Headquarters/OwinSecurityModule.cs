using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.Providers;
using WB.Core.BoundedContexts.Headquarters.Views.User;
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
            registry.Bind<IPasswordHasher<HqUser>, HqPasswordHasher>();
            registry.Bind<IIdentityValidator, HqUserValidator>();

            registry.Bind<HqUserStore, HqUserStore>();

            
            registry.Bind<IApiTokenProvider, ApiAuthTokenProvider>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
            => Task.CompletedTask;
    }
}
