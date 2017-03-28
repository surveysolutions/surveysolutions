using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class OwinSecurityModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<HQIdentityDbContext>().ToMethod(c => HQIdentityDbContext.Create());
            this.Bind<IUserRepository>().To<HqUserStore>();
            this.Bind<IHashCompatibilityProvider>().To<HashCompatibilityProvider>().InSingletonScope();
            this.Bind<IPasswordHasher>().To<PasswordHasher>();
            this.Bind<IIdentityValidator<string>>().To<HqPasswordValidator>();
            this.Bind<IOwinContext>().ToMethod(context => new HttpContextWrapper(HttpContext.Current).GetOwinContext()).InRequestScope();
            this.Bind<IAuthenticationManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Authentication).InRequestScope();

            this.Bind<HqSignInManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Get<HqSignInManager>()).InRequestScope();

            this.Bind<HqUserManager>().ToMethod(context =>
            {
                var ctx = HttpContext.Current == null ? null : context.Kernel.Get<IOwinContext>();

                return ctx == null 
                ? HqUserManager.Create(null, context.Kernel.Get<HQIdentityDbContext>()) 
                : ctx.Get<HqUserManager>();
            }).InRequestScope();

            this.Bind<IAuthorizedUser>().To<AuthorizedUser>().InRequestScope();
        }
    }
}