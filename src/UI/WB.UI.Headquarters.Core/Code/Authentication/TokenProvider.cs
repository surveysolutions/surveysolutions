using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class TokenProvider : ITokenProvider
    {
        private IOptions<TokenProviderOptions> TokenProviderOptions { get; }

        public TokenProvider(IOptions<TokenProviderOptions> tokenProviderOptions)
        {
            TokenProviderOptions = tokenProviderOptions;
        }

        public bool CanGenerate => TokenProviderOptions.Value.IsBearerEnabled;

        public string GetBearerToken(HqUser user)
        {

            if (!TokenProviderOptions.Value.IsBearerEnabled)
                return string.Empty;
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.FormatGuid()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
    }
    //
    // private readonly TokenProviderOptions tokenProviderOptions = new TokenProviderOptions()
    // {
    // Audience = "All",
    // Issuer = "Survey.Solutions",
    // SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("JsfsFpzYcegECGH53lerSgEecwrcRGaH")), SecurityAlgorithms.HmacSha256)
    // };
}
