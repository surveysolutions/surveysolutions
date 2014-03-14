using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using NSubstitute;
using Raven.Client.Document;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.Core.Infrastructure.Raven.Implementation.PlainStorage;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests
{
    public static class Create
    {
        public static AccountController AccountController(UserManager<ApplicationUser> userManager = null,
            IAuthenticationManager authenticationManager = null)
        {
            if (userManager == null)
            {
                userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>());
            }

            if (authenticationManager == null)
            {
                authenticationManager = Substitute.For<IAuthenticationManager>();
            }

            return new AccountController(userManager, authenticationManager);
        }

        public static UsersController UsersController(UserManager<ApplicationUser> userManager = null, 
            DocumentStore storageProvider = null)
        {
            if (userManager == null)
            {
                userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>());
            }

            RavenPlainStorageProvider store = storageProvider == null ? 
                new RavenPlainStorageProvider(storageProvider) : 
                new RavenPlainStorageProvider(storageProvider);

            return new UsersController(userManager, store);
        }
    }
}