using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IUserRepository : IDisposable
    {
        DbSet<DeviceSyncInfo> DeviceSyncInfos { get; }
        Task<string> GetSecurityStampAsync(HqUser user);
        Task CreateAsync(HqUser user);
        Task UpdateAsync(HqUser user);
        Task<HqUser> FindByIdAsync(Guid userId);
        Task<HqUser> FindByNameAsync(string userName);
        HqUser FindById(Guid userId);
        Task SetPasswordHashAsync(HqUser user, string hash);
        Task SetSecurityStampAsync(HqUser user, string newSecurityStamp);
        Task SetLockoutEnabledAsync(HqUser user, bool isLockout);
        IQueryable<HqUser> Users { get; }
        Task<IList<string>> GetRolesAsync(Guid userId);
        Task<IEnumerable<Claim>> GetClaimsAsync(Guid userId);
        Task<string> GetEmailAsync(HqUser user);
        Task<HqUser> FindByEmailAsync(string email);
    }
}
