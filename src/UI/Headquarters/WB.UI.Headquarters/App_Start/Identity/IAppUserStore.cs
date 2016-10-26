using System;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Identity
{
    public interface IAppUserStore : IUserStore<ApplicationUser, Guid>
    {

    }
}