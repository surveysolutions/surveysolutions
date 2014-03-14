using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using Raven.Client.Document;
using Raven.Client.Embedded;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.Core.Infrastructure.Raven.Implementation.PlainStorage;
using WB.Core.Infrastructure.Raven.PlainStorage;
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
                userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>()).Object;
            }

            if (authenticationManager == null)
            {
                authenticationManager = Mock.Of<IAuthenticationManager>();
            }

            return new AccountController(userManager, authenticationManager);
        }

        public static UsersController UsersController(UserManager<ApplicationUser> userManager = null, 
            DocumentStore storageProvider = null)
        {
            if (userManager == null)
            {
                userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>()).Object;
            }

            RavenPlainStorageProvider store = storageProvider == null ? 
                new RavenPlainStorageProvider(storageProvider) : 
                new RavenPlainStorageProvider(storageProvider);

            return new UsersController(userManager, store);
        }
    }
}