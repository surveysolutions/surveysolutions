using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IUserRepository : IAppUserStore
    {
        IQueryable<ApplicationUser> Users { get; }
    }
}
