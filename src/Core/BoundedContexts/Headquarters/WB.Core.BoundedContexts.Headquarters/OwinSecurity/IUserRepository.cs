using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IUserRepository : IQueryableUserStore<HqUser, Guid>
    {
        IDbSet<DeviceSyncInfo> DeviceSyncInfos { get; }
    }
}
