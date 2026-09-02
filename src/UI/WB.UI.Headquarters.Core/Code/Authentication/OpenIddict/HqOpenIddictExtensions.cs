using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace WB.UI.Headquarters.Code.Authentication.OpenIddict
{
    public static class HqOpenIddictExtensions
    {
        public static void AddHqOpenIddict(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            var settings = configuration.GetSection("OpenIddict").Get<HqOpenIddictOptions>() ?? new HqOpenIddictOptions();
            services.Configure<HqOpenIddictOptions>(configuration.GetSection("OpenIddict"));

            if (!settings.Enabled)
                return;

            if (!Uri.TryCreate(settings.Issuer, UriKind.Absolute, out var issuer) ||
                !string.IsNullOrEmpty(issuer.Query) || !string.IsNullOrEmpty(issuer.Fragment) ||
                (!environment.IsDevelopment() && issuer.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException("OpenIddict:Issuer must be a stable absolute HTTPS URL.");

            if (string.IsNullOrWhiteSpace(settings.SigningCertificatePath))
                throw new InvalidOperationException("OpenIddict:SigningCertificatePath is required when OpenIddict is enabled.");

            var certificate = new X509Certificate2(settings.SigningCertificatePath, settings.SigningCertificatePassword);
            if (!certificate.HasPrivateKey)
                throw new InvalidOperationException("OpenIddict signing certificate must contain a private key.");

            services.AddOpenIddict()
                .AddServer(options =>
                {
                    options.SetIssuer(issuer);
                    options.SetAuthorizationEndpointUris("/connect/authorize");
                    options.SetTokenEndpointUris("/connect/token");
                    options.SetUserInfoEndpointUris("/connect/userinfo");
                    options.SetEndSessionEndpointUris("/connect/logout");
                    options.AllowAuthorizationCodeFlow();
                    options.RequireProofKeyForCodeExchange();
                    options.RegisterScopes(OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Roles);
                    options.SetAuthorizationCodeLifetime(settings.AuthorizationCodeLifetime);
                    options.SetIdentityTokenLifetime(settings.IdentityTokenLifetime);
                    options.SetAccessTokenLifetime(settings.AccessTokenLifetime);
                    options.AddSigningCertificate(certificate);
                    options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableEndSessionEndpointPassthrough()
                        .EnableUserInfoEndpointPassthrough();
                });
        }
    }
}
