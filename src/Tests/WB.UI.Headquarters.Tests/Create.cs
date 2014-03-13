using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
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
    }
}