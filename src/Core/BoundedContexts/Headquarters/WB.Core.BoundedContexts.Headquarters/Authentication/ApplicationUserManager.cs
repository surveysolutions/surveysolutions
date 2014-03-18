using System;
using System.Configuration;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;

namespace WB.Core.BoundedContexts.Headquarters.Authentication
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {

            UserValidator = new UserValidator<ApplicationUser>(this)
            {
                AllowOnlyAlphanumericUserNames = false
            };

            int requiredLength = Int32.Parse(ConfigurationManager.AppSettings["MinPasswordLength"]);
            PasswordValidator = new CustomPasswordValidator(requiredLength, ConfigurationManager.AppSettings["PasswordPattern"]);
        }
    }
}