using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IUserRepository : IUserStore<HqUser, Guid>
    {
        IQueryable<HqUser> Users { get; }
    }
}
