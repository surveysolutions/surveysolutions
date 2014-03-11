using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Identity.RavenDB.Entities;
using AspNet.Identity.RavenDB.Stores;
using Microsoft.AspNet.Identity;
using Ninject;
using Ninject.Web.Common;
using Ninject.Modules;
using Raven.Client;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.Core.Infrastructure.Raven.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Authentication
{
    public class AuthenticationModule : NinjectModule
    {
        public override void Load()
        {

            Kernel.Bind<IUserStore<ApplicationUser>>()
                .ToMethod(context => new RavenUserStore<ApplicationUser>(GetSession(), true)).InRequestScope();


            Kernel.Bind<UserManager<ApplicationUser>>().ToSelf().InTransientScope();


            this.RegisterFirstAdmin();
        }

        private void RegisterFirstAdmin()
        {
            var userManager = this.Kernel.Get<UserManager<ApplicationUser>>();
            var applicationUser = new ApplicationUser
            {
                UserName = "Admin"
            };
            var adminRole = new RavenUserClaim(new Claim(ClaimTypes.Role, ApplicationRoles.Administrator));
            applicationUser.Claims.Add(adminRole);
            userManager.CreateAsync(applicationUser, "123456");
        }

        private IAsyncDocumentSession GetSession()
        {
            return this.Kernel.Get<IRavenPlainStorageProvider>().GetDocumentStore().OpenAsyncSession();
        }
    }
}