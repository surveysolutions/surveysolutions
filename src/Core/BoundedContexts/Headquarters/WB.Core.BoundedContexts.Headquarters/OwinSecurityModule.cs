using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class OwinSecurityModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<HQIdentityDbContext>().ToConstant(HQIdentityDbContext.Create());
            this.Bind<IUserRepository>().To<AppUserStore>();

            this.Bind<IPasswordHasher>().To<AspNetPasswordHasher>();
            this.Bind<IOwinContext>().ToMethod(context => new HttpContextWrapper(HttpContext.Current).GetOwinContext()).InRequestScope();
            this.Bind<IAuthenticationManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Authentication).InRequestScope();
            this.Bind<ApplicationSignInManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Get<ApplicationSignInManager>()).InRequestScope();
            this.Bind<ApplicationUserManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Get<ApplicationUserManager>()).InRequestScope();
            this.Bind<IIdentityManager>().To<IdentityManager>().InRequestScope();
        }
    }
}