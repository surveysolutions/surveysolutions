using System;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    internal class AppUserStore : UserStore<ApplicationUser, AppRole, Guid, AppUserLogin, AppUserRole, AppUserClaim>, IUserRepository
    {
        public AppUserStore() : base(new HQIdentityDbContext())
        {

        }

        public AppUserStore(HQIdentityDbContext context) : base(context)
        {

        }
    }
}