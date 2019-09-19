using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.User
{
    class UserStorage : IUserStorage
    {
        private readonly IMemoryCache memoryCache;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;

        public UserStorage(IMemoryCache memoryCache, ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.memoryCache = memoryCache;
            this.tenantApi = tenantApi;
        }

        public async Task<string> GetUserNameAsync(TenantInfo tenantInfo, Guid userId)
        {
            var headquartersApi = tenantApi.For(tenantInfo);
            var user = await GetUserAsync(headquartersApi, userId);
            return user.UserName;
        }

        public async Task<UserRoles> GetUserRoleAsync(TenantInfo tenantInfo, Guid userId)
        {
            var headquartersApi = tenantApi.For(tenantInfo);
            var user = await GetUserAsync(headquartersApi, userId);
            return user.UserRole;
        }

        private Task<User> GetUserAsync(IHeadquartersApi headquartersApi, Guid userId)
        {
            return memoryCache.GetOrCreateAsync(userId,
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(3);
                    User user = await headquartersApi.GetUser(userId);
                    return user;
                });
        }
    }
}
