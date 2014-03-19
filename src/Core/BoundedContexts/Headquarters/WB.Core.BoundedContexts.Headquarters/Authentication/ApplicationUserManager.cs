using System;
using System.Configuration;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;

namespace WB.Core.BoundedContexts.Headquarters.Authentication
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store, ApplicationUserManagerSettings settings)
            : base(store)
        {

            UserValidator = new UserValidator<ApplicationUser>(this)
            {
                AllowOnlyAlphanumericUserNames = false
            };

            PasswordValidator = new CustomPasswordValidator(settings.MinPasswordLength, settings.PasswordPattern);
        }
    }
}