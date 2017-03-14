using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqUserManager : UserManager<HqUser, Guid>
    {
        public HqUserManager(IUserStore<HqUser, Guid> store)
            : base(store)
        {
        }

        public static HqUserManager Create(IdentityFactoryOptions<HqUserManager> options,
            IOwinContext context)
        {
            var manager = new HqUserManager(new HqUserStore(context.Get<HQIdentityDbContext>()))
            {
                PasswordHasher = ServiceLocator.Current.GetInstance<IPasswordHasher>(),
                UserLockoutEnabledByDefault = false,
                DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5),
                MaxFailedAccessAttemptsBeforeLockout = 5
            };
            manager.UserValidator = new UserValidator<HqUser, Guid>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };

            return manager;
        }
    }
}