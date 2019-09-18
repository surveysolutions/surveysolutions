using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Services;

namespace WB.Services.Export.User
{
    public interface IUserStorage
    {
        Task<string> GetUserNameAsync(Guid userId);
        Task<UserRoles> GetUserRoleAsync(Guid userId);
    }

    class UserStorage : IUserStorage
    {
        private readonly IMemoryCache memoryCache;
        private readonly IHeadquartersApi headquartersApi;

        public UserStorage(IMemoryCache memoryCache, IHeadquartersApi headquartersApi)
        {
            this.memoryCache = memoryCache;
            this.headquartersApi = headquartersApi;
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
                    User user = await headquartersApi.GetUser(userId);
                    return user;
                });
        }
    }
}
