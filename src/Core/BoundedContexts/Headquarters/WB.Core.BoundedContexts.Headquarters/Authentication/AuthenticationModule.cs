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
                .ToMethod(context => new RavenUserStore<ApplicationUser>(GetSession(), false)).InRequestScope();

            Kernel.Bind<UserManager<ApplicationUser>>().ToSelf().InTransientScope();
        }

        private IAsyncDocumentSession GetSession()
        {
            return this.Kernel.Get<IRavenPlainStorageProvider>().GetDocumentStore().OpenAsyncSession();
        }
    }
}