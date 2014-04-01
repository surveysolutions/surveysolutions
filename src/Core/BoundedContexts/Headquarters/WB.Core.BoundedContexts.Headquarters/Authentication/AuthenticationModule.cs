using System;
using AspNet.Identity.RavenDB.Stores;
using Microsoft.AspNet.Identity;
using Ninject;
using Ninject.Web.Common;
using Ninject.Modules;
using Raven.Client;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.Core.BoundedContexts.Headquarters.PasswordPolicy;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Raven.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Authentication
{
    public class AuthenticationModule : NinjectModule
    {
        public override void VerifyRequiredModulesAreLoaded()
        {
            if (!this.Kernel.HasModule(typeof(PasswordPolicyModule).FullName))
            {
                throw new InvalidOperationException("PasswordPolicyModule is required");
            }
        }

        public override void Load()
        {
            this.Kernel.Bind<IUserStore<ApplicationUser>>()
                .ToMethod(context => new RavenUserStore<ApplicationUser>(this.GetSession())).InRequestScope();

            Kernel.Bind<UserManager<ApplicationUser>>().To<ApplicationUserManager>().InTransientScope();

            this.RegisterFirstAdmin();
        }

        private void RegisterFirstAdmin()
        {
            var userManager = this.Kernel.Get<UserManager<ApplicationUser>>();
            var applicationUser = new ApplicationUser(Guid.NewGuid().FormatGuid())
            {
                UserName = "Admin",
                IsAdministrator = true
            };
            userManager.CreateAsync(applicationUser, "P@$$w0rd");
        }

        private IAsyncDocumentSession GetSession()
        {
            return this.Kernel.Get<IRavenPlainStorageProvider>().GetDocumentStore().OpenAsyncSession();
        }
    }
}