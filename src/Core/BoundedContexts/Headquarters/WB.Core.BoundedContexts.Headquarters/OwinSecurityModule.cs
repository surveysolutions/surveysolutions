using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using IPasswordHasher = Microsoft.AspNet.Identity.IPasswordHasher;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class OwinSecurityModule : NinjectModule
    {
        private IOwinContext owinContext => new HttpContextWrapper(HttpContext.Current).GetOwinContext();

        public override void Load()
        {
            this.Bind<IPasswordHasher>().To<AspNetPasswordHasher>();
            this.Bind<IAuthenticationManager>().ToMethod(context => this.owinContext.Authentication);

            this.Bind<ApplicationSignInManager>().ToMethod(context => this.owinContext.Get<ApplicationSignInManager>());
            this.Bind<ApplicationUserManager>().ToMethod(context => this.owinContext.Get<ApplicationUserManager>());

            this.Bind<IIdentityManager>().To<IdentityManager>();
        }
    }
}