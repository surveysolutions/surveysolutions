using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers
{
    /// <summary>
    /// Api AuthToken provider using Microsoft.Identity PasswordHasher. Instead of password SecurityStamp is used
    /// This way we will be able to generate random tokens linked to single user.
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="WB.Core.BoundedContexts.Headquarters.OwinSecurity.IApiTokenProvider{TKey}" />
    public class ApiAuthTokenProvider<TUser, TKey>: IApiTokenProvider<TKey>
        where TUser : class, IUser<TKey> where TKey : IEquatable<TKey>
    {
        private readonly UserManager<TUser, TKey> manager;
        private readonly PasswordHasher hasher;

        public ApiAuthTokenProvider(UserManager<TUser, TKey> userManager)
        {
            this.manager = userManager;
            this.hasher = new PasswordHasher();
        }

        public async Task<string> GenerateTokenAsync(TKey userId)
        {
            var securityStamp = await this.manager.GetSecurityStampAsync(userId);
            return this.hasher.HashPassword(securityStamp); 
        }

        private static readonly ConcurrentDictionary<(string stamp, string token), bool> HashCache 
            = new ConcurrentDictionary<(string, string), bool>();

        public async Task<bool> ValidateTokenAsync(TKey userId, string token)
        {
            var securityStamp = await this.manager.GetSecurityStampAsync(userId);

            if(HashCache.Count > 100_000) HashCache.Clear();

            return HashCache.GetOrAdd((securityStamp, token), 
                t => this.hasher.VerifyHashedPassword(t.token, t.stamp) == PasswordVerificationResult.Success);
        }
    }
}