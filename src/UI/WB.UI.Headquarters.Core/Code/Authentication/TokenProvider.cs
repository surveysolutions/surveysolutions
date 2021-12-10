using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class TokenProvider : ITokenProvider
    {
        private const string ProviderName = "HQTokenProvider";
        private const string TokenName = "Jti";
        
        
        private readonly HqUserManager userManager;
        private IOptions<TokenProviderOptions> TokenProviderOptions { get; }

        public TokenProvider(IOptions<TokenProviderOptions> tokenProviderOptions, HqUserManager userManager)
        {
            TokenProviderOptions = tokenProviderOptions;
            this.userManager = userManager;
        }

        public bool CanGenerate => TokenProviderOptions.Value.IsBearerEnabled;

        public async Task<string> GetOrCreateBearerTokenAsync(HqUser user)
        {
            if (!TokenProviderOptions.Value.IsBearerEnabled)
                return string.Empty;

            var jtiToken = await this.userManager.GetAuthenticationTokenAsync(user, ProviderName, TokenName);

            if (string.IsNullOrEmpty(jtiToken))
            {
                jtiToken = Guid.NewGuid().FormatGuid();
                await this.userManager.SetAuthenticationTokenAsync(user, ProviderName, TokenName, jtiToken);
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.FormatGuid()),
                new Claim(JwtRegisteredClaimNames.Jti, jtiToken)
            };

            var token = new JwtSecurityToken(TokenProviderOptions.Value.Issuer,
                TokenProviderOptions.Value.Audience,
                claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(TokenProviderOptions.Value.Expiration),
                signingCredentials: TokenProviderOptions.Value.SigningCredentials);

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(token);

            return encodedJwt;
        }

        public async Task InvalidateBearerTokenAsync(HqUser user)
        {
            await this.userManager.RemoveAuthenticationTokenAsync(user, ProviderName, TokenName);
        }

        public async Task<bool> DoesTokenExist(HqUser user)
        {
            return !string.IsNullOrEmpty(await this.userManager.GetAuthenticationTokenAsync(user, ProviderName, TokenName));
        }

        public async Task<bool> ValidateJtiAsync(HqUser user, string jti)
        {
            var token =  await this.userManager.GetAuthenticationTokenAsync(user, ProviderName, TokenName);

            if (string.IsNullOrEmpty(token))
                return false;
            
            return string.Compare(token, jti, StringComparison.CurrentCultureIgnoreCase) == 0;
        }
    }
}
