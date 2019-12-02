using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public interface IUserRepository
    {
        IQueryable<HqUser> Users { get; }
        IQueryable<DeviceSyncInfo> DeviceSyncInfos { get; }
        HqRole FindRole(Guid id);

        Task<string> GetSecurityStampAsync(HqUser user);
        Task<Microsoft.AspNetCore.Identity.IdentityResult> CreateAsync(HqUser user, CancellationToken cancellationToken = new CancellationToken());
        Task<Microsoft.AspNetCore.Identity.IdentityResult> UpdateAsync(HqUser user, CancellationToken cancellationToken = new CancellationToken());
        Task<HqUser> FindByIdAsync(Guid userId, CancellationToken cancellationToken = new CancellationToken());
        Task<HqUser> FindByNameAsync(string userName, CancellationToken cancellationToken = new CancellationToken());
        HqUser FindById(Guid userId);
        Task SetPasswordHashAsync(HqUser user, string hash);
        Task SetSecurityStampAsync(HqUser user, string newSecurityStamp);
        Task SetLockoutEnabledAsync(HqUser user, bool isLockout);
        Task<IList<string>> GetRolesAsync(Guid userId);
        Task<IEnumerable<Claim>> GetClaimsAsync(Guid userId);
        Task<string> GetEmailAsync(HqUser user);
        Task<HqUser> FindByEmailAsync(string email);
    }
}
