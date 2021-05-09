using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.User
{
    class UserStorage : IUserStorage
    {
        private readonly IMemoryCache memoryCache;
        private readonly ITenantContext tenantContext;

        public UserStorage(IMemoryCache memoryCache, ITenantContext tenantContext)
        {
            this.memoryCache = memoryCache;
            this.tenantContext = tenantContext;
        }

        public async Task<string> GetUserNameAsync(Guid userId)
        {
            var user = await GetUserAsync(userId);
            return user.UserName;
        }

        public async Task<UserRoles> GetUserRoleAsync(Guid userId)
        {
            var user = await GetUserAsync(userId);
            return user.Roles.Single();
        }

        private Task<ServicesIntegration.Export.User> GetUserAsync(Guid userId)
        {
            return memoryCache.GetOrCreateAsync(userId,
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(3);
                    ServicesIntegration.Export.User user = await tenantContext.Api.GetUserAsync(userId);
                    return user;
                });
        }
    }
}
