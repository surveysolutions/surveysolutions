using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class OwinSecurityModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IUserRepository>().To<HqUserStore>();
            this.Bind<IHashCompatibilityProvider>().To<HashCompatibilityProvider>().InSingletonScope();
            this.Bind<IPasswordHasher>().To<PasswordHasher>();
            this.Bind<IIdentityValidator<string>>().To<HqPasswordValidator>();

            this.Bind<IOwinContext>().ToMethod(context => new HttpContextWrapper(HttpContext.Current).GetOwinContext());
            this.Bind<IAuthenticationManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Authentication);

            // no on per request scope required - lifetime managed by their parents controllers/handlers
            this.Bind<HQIdentityDbContext>().ToSelf();
            this.Kernel.Get<HQPlainStorageDbContext>().DeviceSyncInfo.FirstOrDefault();

            this.Bind<HQPlainStorageDbContext>().ToSelf();
            this.Kernel.Get<HQIdentityDbContext>().Roles.FirstOrDefault();

            this.Bind<IUserStore<HqUser, Guid>>().To<HqUserStore>();
            this.Bind<HqUserManager>().ToSelf();
            this.Bind<HqSignInManager>().ToSelf();

            this.Bind<IApiTokenProvider<Guid>>().To<ApiAuthTokenProvider<HqUser, Guid>>();
            this.Bind<IAuthorizedUser>().To<AuthorizedUser>();
        }
    }
}