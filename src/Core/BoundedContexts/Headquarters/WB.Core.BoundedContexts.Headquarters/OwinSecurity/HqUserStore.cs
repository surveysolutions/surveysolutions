using System;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    internal class HqUserStore : UserStore<HqUser, HqRole, Guid, HqUserLogin, HqUserRole, HqUserClaim>, IUserRepository
    {
        public HqUserStore(HQIdentityDbContext context) : base(context)
        {
            this.DbContext = context;
        }

        public HQIdentityDbContext DbContext { get; }
    }
}