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
    public class DelegatedTokenRequest
    {
        public string? UserId { get; init; }
        public string CorrelationId { get; init; } = "";
        public Guid QuestionnaireId { get; init; }
        public string AuthorizedParty { get; init; } = "WB.WebTester";
        public string Scope { get; init; } = "webtester";
    }

    public interface IDelegatedTokenService
    {
        string CreateDelegatedToken(DelegatedTokenRequest request);
    }

    /// <summary>
    /// Issues a short-lived delegated JWT for Service B (WebTester) → Service A (Designer) backend calls.
    /// The token is scoped to a single questionnaire and a specific authorized party.
    /// </summary>
    public class DelegatedTokenService : IDelegatedTokenService
    {
        public const string DelegatedAudience = "WB.Designer";

        private readonly WebTesterSettings settings;
        private readonly IConfiguration configuration;

        public DelegatedTokenService(IOptions<WebTesterSettings> settings, IConfiguration configuration)
        {
            this.settings = settings.Value;
            this.configuration = configuration;
        }

        public string CreateDelegatedToken(DelegatedTokenRequest request)
        {
            var secretKey = configuration["WebTester:JwtSecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                secretKey = configuration["Providers:Assistant:JwtSecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("JWT secret key is not configured.");

            var expirationMinutes = settings.DelegatedJwtExpirationMinutes > 0
                ? settings.DelegatedJwtExpirationMinutes
                : 10;

            var now = DateTime.UtcNow;
            var jti = Guid.NewGuid().ToString();
            var issuer = configuration["Providers:Assistant:JwtIssuer"] ?? "WB.Designer";

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

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: DelegatedAudience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
