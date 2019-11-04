using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class OwinSecurityModule : IModule, IInitModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IUserRepository, HqUserStore>();
            registry.Bind<IHashCompatibilityProvider, HashCompatibilityProvider>();
            registry.Bind<IIdentityPasswordHasher, HqPasswordHasher>();
            registry.Bind<IPasswordValidator, HqPasswordValidator>();
            registry.Bind<IIdentityValidator, HqUserValidator>();

            // no on per request scope required - lifetime managed by their parents controllers/handlers
            registry.Bind<HQIdentityDbContext>();

            registry.Bind<HQPlainStorageDbContext>();

            registry.Bind<IUserRepository, HqUserStore>();

            
            registry.Bind<HqUserManager>();

            registry.Bind<IApiTokenProvider, ApiAuthTokenProvider>();
            registry.Bind<IAuthorizedUser, AuthorizedUser>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
            => Task.CompletedTask;
    }
}
