﻿using System;
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

        public async Task<string> GetUserNameAsync(Guid userId)
        {
            var user = await GetUserAsync(userId);
            return user.UserName;
        }

        public async Task<UserRoles> GetUserRoleAsync(Guid userId)
        {
            var user = await GetUserAsync(userId);
            return user.UserRole;
        }

        private Task<User> GetUserAsync(Guid userId)
        {
            return memoryCache.GetOrCreateAsync(userId,
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(3);
                    User user = await tenantContext.Api.GetUser(userId);
                    return user;
                });
        }
    }
}
