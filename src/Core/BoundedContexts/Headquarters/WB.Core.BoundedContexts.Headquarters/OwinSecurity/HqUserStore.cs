using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    internal class HqUserStore : UserStore<HqUser, HqRole, Guid, HqUserLogin, HqUserRole, HqUserClaim>, IUserRepository
    {
        private readonly HQIdentityDbContext context;

        public HqUserStore(HQIdentityDbContext context) : base(context)
        {
            this.context = context;
        }

        public IDbSet<DeviceSyncInfo> DeviceSyncInfos => context.DeviceSyncInfos;
    }
}