using System;
using System.Linq;
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
        private readonly ITenantContext tenantContext;

        public UserStorage(IMemoryCache memoryCache, ITenantContext tenantContext)
        {
            this.memoryCache = memoryCache;
            this.tenantContext = tenantContext;
        }

        public Task<User?> GetUserAsync(Guid userId)
        {
            return memoryCache.GetOrCreateAsync(userId,
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(3);
                    User user = await tenantContext.Api.GetUserAsync(userId);
                    return user;
                });
        }
    }
}
