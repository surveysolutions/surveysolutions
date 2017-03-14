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
            this.Bind<IUserRepository>().To<HqUserStore>();

            this.Bind<IPasswordHasher>().To<HqPasswordHasher>();
            this.Bind<IOwinContext>().ToMethod(context => new HttpContextWrapper(HttpContext.Current).GetOwinContext()).InRequestScope();
            this.Bind<IAuthenticationManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Authentication).InRequestScope();
            this.Bind<HqSignInManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Get<HqSignInManager>()).InRequestScope();
            this.Bind<HqUserManager>().ToMethod(context => context.Kernel.Get<IOwinContext>().Get<HqUserManager>()).InRequestScope();
            this.Bind<IIdentityManager>().To<IdentityManager>().InRequestScope();
        }
    }
}