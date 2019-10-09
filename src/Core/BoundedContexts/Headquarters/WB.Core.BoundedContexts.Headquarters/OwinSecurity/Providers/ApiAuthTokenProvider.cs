using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers
{
    /// <summary>
    /// Api AuthToken provider using Microsoft.Identity PasswordHasher. Instead of password SecurityStamp is used
    /// This way we will be able to generate random tokens linked to single user.
    /// </summary>
    public class ApiAuthTokenProvider: IApiTokenProvider
    {
        private readonly HqUserManager manager;
        public ApiAuthTokenProvider(HqUserManager userManager)
        {
            this.manager = userManager;
        }

        public async Task<string> GenerateTokenAsync(Guid userId)
        {
            var securityStamp = await this.manager.GetSecurityStampAsync(userId);
            return this.manager.PasswordHasher.Hash(securityStamp); 
        }

        private static readonly ConcurrentDictionary<(string stamp, string token), bool> HashCache 
            = new ConcurrentDictionary<(string, string), bool>();

        public async Task<bool> ValidateTokenAsync(Guid userId, string token)
        {
            var securityStamp = await this.manager.GetSecurityStampAsync(userId);

            if(HashCache.Count > 100_000) HashCache.Clear();

            return HashCache.GetOrAdd((securityStamp, token), 
                t => this.manager.PasswordHasher.VerifyPassword(t.token, t.stamp) == PasswordVerificationResult.Success);
        }
    }
}
