using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using PasswordVerificationResult = WB.Core.GenericSubdomains.Portable.PasswordVerificationResult;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
{
    /// <summary>
    /// Api AuthToken provider using Microsoft.Identity PasswordHasher. Instead of password SecurityStamp is used
    /// This way we will be able to generate random tokens linked to single user.
    /// </summary>
    public class ApiAuthTokenProvider: IApiTokenProvider
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly HqUserManager manager;
        public ApiAuthTokenProvider(IPasswordHasher passwordHasher,
            HqUserManager userManager)
        {
            this.passwordHasher = passwordHasher;
            this.manager = userManager;
        }

        public async Task<string> GenerateTokenAsync(Guid userId)
        {
            var user = await this.manager.FindByIdAsync(userId.FormatGuid());
            var securityStamp = await this.manager.GetSecurityStampAsync(user);
            return passwordHasher.Hash(securityStamp); 
        }

        private static readonly ConcurrentDictionary<(string stamp, string token), bool> HashCache 
            = new ConcurrentDictionary<(string, string), bool>();

        public async Task<bool> ValidateTokenAsync(Guid userId, string token)
        {
            var user = await this.manager.FindByIdAsync(userId.FormatGuid());
            var securityStamp = await this.manager.GetSecurityStampAsync(user);

            if(HashCache.Count > 100_000) HashCache.Clear();

            return HashCache.GetOrAdd((securityStamp, token), 
                t => passwordHasher.VerifyPassword(t.token, t.stamp) == PasswordVerificationResult.Success);
        }
    }
}
