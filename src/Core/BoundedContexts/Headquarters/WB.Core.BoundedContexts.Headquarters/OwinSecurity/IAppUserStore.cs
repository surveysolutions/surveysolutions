using System;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IAppUserStore : IUserStore<ApplicationUser, Guid>
    {

    }
}