using System;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IUserRepository : IQueryableUserStore<HqUser, Guid>
    {
        HQIdentityDbContext DbContext { get; }
    }
}
