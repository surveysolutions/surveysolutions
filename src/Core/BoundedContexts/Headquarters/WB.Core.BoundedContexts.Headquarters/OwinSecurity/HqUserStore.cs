using System;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    internal class HqUserStore : UserStore<HqUser, HqRole, Guid, HqUserLogin, HqUserRole, HqUserClaim>, IUserRepository
    {
        public HqUserStore() : base(new HQIdentityDbContext())
        {

        }

        public HqUserStore(HQIdentityDbContext context) : base(context)
        {

        }
    }
}