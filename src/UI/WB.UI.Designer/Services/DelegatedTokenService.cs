using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WB.UI.Designer.Services
{

    /// <summary>
    /// Issues a short-lived delegated JWT for Service B (WebTester) → Service A (Designer) backend calls.
    /// The token is scoped to a single questionnaire and a specific authorized party.
    /// </summary>
    public class DelegatedTokenService : IDelegatedTokenService
    {
        public const string DelegatedAudience = "WB.Designer";

        /// <summary>Authentication scheme used exclusively by WebTester delegated-token endpoints.</summary>
        public const string DelegatedScheme = "webtester-delegated";

        private readonly WebTesterSettings settings;
        private readonly SigningCredentials signingCredentials;
        private readonly string issuer;

        public DelegatedTokenService(IOptions<WebTesterSettings> settings, IConfiguration configuration)
        {
            this.settings = settings.Value;

            var secretKey = configuration["WebTester:JwtSecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException(
                    "JWT secret key is not configured. " +
                    "Set WebTester:JwtSecretKey.");

            issuer = configuration["Providers:Assistant:JwtIssuer"] ?? "WB.Designer";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }

        public string CreateDelegatedToken(DelegatedTokenRequest request)
        {
            var expirationMinutes = settings.DelegatedJwtExpirationMinutes > 0
                ? settings.DelegatedJwtExpirationMinutes
                : 10;

            var now = DateTime.UtcNow;
            var jti = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim("azp", request.AuthorizedParty),
                new Claim("scope", request.Scope),
                new Claim("correlation_id", request.CorrelationId),
                new Claim(JwtTokenService.QuestionnaireIdClaimType, request.QuestionnaireId.ToString())
            };

            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, request.UserId));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, request.UserId));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: DelegatedAudience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(expirationMinutes),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
