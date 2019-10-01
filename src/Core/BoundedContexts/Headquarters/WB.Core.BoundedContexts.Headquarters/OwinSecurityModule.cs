using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
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
            registry.Bind<IPasswordHasher, PasswordHasher>();
            registry.Bind<IIdentityValidator<string>, HqPasswordValidator>();

            // no on per request scope required - lifetime managed by their parents controllers/handlers
            registry.Bind<HQIdentityDbContext>();

            registry.Bind<HQPlainStorageDbContext>();

            registry.Bind<IUserStore<HqUser, Guid>, HqUserStore>();

            
            registry.Bind<HqUserManager>();

            registry.Bind<UserManager<HqUser, Guid>, HqUserManager>();


            registry.Bind<IApiTokenProvider<Guid>, ApiAuthTokenProvider<HqUser, Guid>>();
            registry.Bind<IAuthorizedUser, AuthorizedUser>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            serviceLocator.GetInstance<HQPlainStorageDbContext>().DeviceSyncInfo.FirstOrDefault();
            serviceLocator.GetInstance<HQIdentityDbContext>().Roles.FirstOrDefault();

            return Task.CompletedTask;
        }
    }
}
