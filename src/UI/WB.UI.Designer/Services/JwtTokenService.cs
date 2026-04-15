using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.UI.Designer.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(DesignerIdentityUser user);
        string GenerateWebTesterToken(DesignerIdentityUser? user, Guid questionnaireId);
    }

    public class JwtTokenService : IJwtTokenService
    {
        public const string WebTesterAudience = "WB.WebTester";
        public const string QuestionnaireIdClaimType = "questionnaire_id";

        private readonly IConfiguration configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GenerateToken(DesignerIdentityUser user)
        {
            var secretKey = configuration["Providers:Assistant:JwtSecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JWT secret key is not configured");
            }

            var issuer = configuration["Providers:Assistant:JwtIssuer"] ?? "WB.Designer";
            var audience = configuration["Providers:Assistant:JwtAudience"] ?? "WB.AssistantService";
            var expirationMinutes = configuration.GetValue<int>("Providers:Assistant:JwtExpirationMinutes");
            if (expirationMinutes == 0)
                expirationMinutes = 30; // default 30 minutes

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return CreateJwtToken(secretKey, issuer, audience, expirationMinutes, claims);
        }

        public string GenerateWebTesterToken(DesignerIdentityUser? user, Guid questionnaireId)
        {
            var secretKey = configuration["WebTester:JwtSecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                secretKey = configuration["Providers:Assistant:JwtSecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JWT secret key is not configured. " +
                    "Set WebTester:JwtSecretKey (or Providers:Assistant:JwtSecretKey) in configuration.");
            }

            var issuer = configuration["Providers:Assistant:JwtIssuer"] ?? "WB.Designer";
            var expirationMinutes = configuration.GetValue<int>("WebTester:JwtExpirationMinutes");
            if (expirationMinutes == 0)
                expirationMinutes = 60; // default 60 minutes for web tester sessions

            var claims = new List<Claim>
            {
                new Claim(QuestionnaireIdClaimType, questionnaireId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user != null)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, user.UserName ?? string.Empty));
            }

            return CreateJwtToken(secretKey, issuer, WebTesterAudience, expirationMinutes, claims);
        }

        private static string CreateJwtToken(string secretKey, string issuer, string audience,
            int expirationMinutes, IEnumerable<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
